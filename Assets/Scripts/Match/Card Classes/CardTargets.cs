using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTargets : MonoBehaviour
{
    GameObject linePrefab;

    public LineRenderer[] lines = new LineRenderer[0];
    public Vector3[] targets = new Vector3[0];
    public int targetAmount;

    // Start is called before the first frame update
    void Start()
    {
        linePrefab = Resources.Load<GameObject>("Prefabs/TargetLine");
    }
    private void Update()
    {
        for(int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null)
                continue;

            if (lines[i].enabled)
            {
                lines[i].positionCount = 2;
                lines[i].SetPositions(new Vector3[] { transform.position, targets[i] });
            }
        }
    }
    public void ResetTargets()
    {
        for(int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
                Destroy(lines[i].gameObject);
        }
    }
    public void SetTargetAmount(int _amount)
    {
        ResetTargets();
        targetAmount = _amount;
        lines = new LineRenderer[targetAmount];
        for(int i = 0; i < targetAmount; i++)
        {
            lines[i] = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
            lines[i].enabled = false;
        }
        targets = new Vector3[targetAmount];
    }
    public void SetTarget(int _index, Vector3 _target)
    {
        targets[_index] = _target;
        lines[_index].enabled = true;
    }
    public void SetTargets(Vector3[] _targets)
    {
        SetTargetAmount(_targets.Length);
        for(int i = 0; i < _targets.Length; i++)
        {
            SetTarget(i, _targets[i]);
        }
    }
}
