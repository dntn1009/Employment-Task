using UnityEngine;

public class BlockOutlineController
{
    private Outline outline;

    public void Init(GameObject go)
    {
        outline = go.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2f;
        outline.enabled = false;
    }

    public void SetActive(bool active)
    {
        if (outline != null) outline.enabled = active;
    }
}
