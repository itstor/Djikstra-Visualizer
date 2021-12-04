using UnityEngine;
using System.Timers;

public class AppManager : MonoBehaviour
{

    public static AppManager Instance = null;
    private GameObject m_newNode = null;
    private NodeState m_SelectedNodeState;
    private readonly Timer m_MouseClickTimer = new Timer();
    private NodeState m_newNodeState;
    [SerializeField] private DialogGUI dialogGUI;
    public bool onSelectedChanged;
    private GameObject m_SelectedNodeProperty;
    public GameObject m_SelectedNode {
        get {
            return this.m_SelectedNodeProperty;
        }

        set {
            if (this.m_SelectedNodeProperty != value){
                onSelectedChanged = true;
            }
            else {
                onSelectedChanged = false;
            }

            this.m_SelectedNodeProperty = value;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        m_MouseClickTimer.Interval = 250;
        m_MouseClickTimer.Elapsed += singleClick;
    }

    void Update()
    {
        if (CursorStateManager.Instance.currentState == states.CursorState.Add)
        {
            if (m_newNode == null)
            {
                m_newNode = ObjectFactory.Instance.createNode(Utils.getMouseWorldPosition());
                m_newNodeState = m_newNode.GetComponent<NodeState>();
                m_newNodeState.onDragAdd();
                
            }

            if (m_newNode != null)
            {
                var mousePosition = Utils.getMouseWorldPosition();
                m_newNode.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);

                if (Input.GetKeyDown(KeyCode.Escape)){
                    Destroy(m_newNode);
                    CursorStateManager.Instance.currentState = states.CursorState.Select;
                    m_newNode = null;
                }
                
                else if (Input.GetMouseButtonDown(0))
                {
                    CursorStateManager.Instance.currentState = states.CursorState.Select;
                    m_newNode.GetComponent<NodeState>().onIdle();
                    dialogGUI.showDialog(0, (string name, GameObject node) =>
                    {
                        node.GetComponent<Node>().nodeName = name;
                    }, m_newNode);
                    m_newNode = null;
                }
            }
        }

        else if (CursorStateManager.Instance.currentState == states.CursorState.Select)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (m_MouseClickTimer.Enabled == false)
                {
                    m_MouseClickTimer.Start();
                    return;
                }
                else
                {
                    m_MouseClickTimer.Stop();

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                    if (hit)
                    {
                        if (hit.collider.gameObject.tag == "Node")
                        {
                            if (m_SelectedNode != null)
                            {
                                m_SelectedNodeState.onIdle();
                                m_SelectedNodeState.toggleForceGlow();
                            }
                            
                            m_SelectedNode = hit.collider.gameObject;
                            m_SelectedNodeState = m_SelectedNode.GetComponent<NodeState>();
                            m_SelectedNodeState.onSelected();
                            m_SelectedNodeState.toggleForceGlow();
                        }
                    }
                    else
                    {
                        if (m_SelectedNode != null)
                        {
                            m_SelectedNodeState.onIdle();
                            m_SelectedNodeState.toggleForceGlow();
                            m_SelectedNode = null;
                        }
                    }
                }
            }
        }

        if (m_SelectedNode != null)
        {
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                GUIManager.Instance.showToast("Deleted " + m_SelectedNode.GetComponent<Node>().nodeName, 2f);
                m_SelectedNode.GetComponent<Node>().deleteNode();
                m_SelectedNode = null;
            }
        }
    }

    void singleClick(object o, System.EventArgs e)
    {
        m_MouseClickTimer.Stop();
    }
}