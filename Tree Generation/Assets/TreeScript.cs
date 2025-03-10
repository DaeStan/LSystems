using System.Collections;
using System.Collections.Generic;
using TreeTutorial;
using UnityEngine;
using UnityEngine.Splines;

public class TreeScript: MonoBehaviour
{
    private string tree;
    [SerializeField]
    private string axiom;
    [SerializeField]
    public int iteration;
    [SerializeField]
    public float length;
    [SerializeField]
    public float angle;
    [SerializeField]
    public Material treeMaterial;

    //UI
    //[SerializeField] 
    //private TMP_InputField iterationInput;
    //[SerializeField] 
    //private TMP_InputField angleInput;
    //[SerializeField] 
    //private TMP_InputField lengthInput;

    private List<List<Vector3>> LineList = new List<List<Vector3>>();

    Stack<TransformFormationHelper> stack = new Stack<TransformFormationHelper>();
    private TransformFormationHelper helper;

    Stack<int> splineIndexStack = new Stack<int>();

    public void Start()
    {
        tree = axiom;
        ExpandTreeString();
        CreateMesh();
    }

    private void OnDrawGizmos()
    {
        foreach(List<Vector3> line in LineList)
        {
            Gizmos.DrawLine(line[0], line[1]);
        }
    }

    void ExpandTreeString()
    {
        string expandTree;

        for (int i = 0; i < iteration; i++)
        {
            expandTree = "";

            foreach (char j in tree)
            {
                expandTree += j switch
                {
                    'F' => "FF",
                    'B' => "[lFB][rBF]",
                    _ => j.ToString()
                };
            }
            tree = expandTree;
        }
    }

    void CreateMesh()
    {
        //Vector3 initalPos;

        GameObject treeObject = new GameObject(name: "TreeObj");
        var meshFilter = treeObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        var meshRenderer = treeObject.AddComponent<MeshRenderer>();
        meshRenderer.material = treeMaterial;

        var container = treeObject.AddComponent<SplineContainer>();
        container.RemoveSplineAt(0);
        var extrude = treeObject.AddComponent<SplineExtrude>();
        extrude.Container = container;

        var currentSpline = container.AddSpline();
        var splineIndex = TreeGeneratorExtension.FindIndex(container.Splines, currentSpline);

        currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);

        foreach (char j in tree)
        {
            switch (j)
            {
                case 'F':
                    transform.Translate(translation: Vector3.up * length);
                    currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);

                    break;
                case 'B':
                    //does nothing
                    break;
                case '[':
                    stack.Push(new TransformFormationHelper()
                    {
                        position = transform.position,
                        rotation = transform.rotation
                    });
                    splineIndexStack.Push(splineIndex);

                    int splineCount = currentSpline.Count;
                    int prevSplineIndex = splineIndex;

                    currentSpline = container.AddSpline();
                    splineIndex = TreeGeneratorExtension.FindIndex(container.Splines, currentSpline);
                    currentSpline.Add(new BezierKnot(transform.position), TangentMode.AutoSmooth);
                    container.LinkKnots(new SplineKnotIndex(prevSplineIndex, knot: splineCount - 1), 
                        new SplineKnotIndex(splineIndex, knot: 0));

                    break;
                case ']':
                    TransformFormationHelper helper = stack.Pop();
                    transform.position = helper.position;
                    transform.rotation = helper.rotation;
                    splineIndex = splineIndexStack.Pop();
                    currentSpline = container.Splines[splineIndex];
                    break;
                case 'l':
                    transform.Rotate(Vector3.back, angle);
                    break;
                case 'r':
                    transform.Rotate(Vector3.forward, angle);
                    break;
            }
        }
    }
}

public static class TreeGeneratorExtension
{
    public static int FindIndex(this IReadOnlyList<Spline> splines, Spline spline)
    {
        for (int i = 0; i < splines.Count; i++)
        {
            if (splines[i] == spline)
            {
                return i;
            }
        }
        return -1;
    }
}