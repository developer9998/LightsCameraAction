using System;
using UnityEngine;

namespace LightsCameraAction.Tools
{
    // Token: 0x0200000E RID: 14
    public class DebugRay : MonoBehaviour
    {
        // Token: 0x06000030 RID: 48 RVA: 0x00003470 File Offset: 0x00001670
        public void Start()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startColor = color;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.material = new Material(UberShader.GetShader());
        }

        // Token: 0x06000031 RID: 49 RVA: 0x000034DF File Offset: 0x000016DF
        public void Set(Vector3 start, Vector3 direction)
        {
            lineRenderer.material.color = color;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start + direction);
        }

        // Token: 0x06000032 RID: 50 RVA: 0x0000351C File Offset: 0x0000171C
        public DebugRay SetColor(Color c)
        {
            color = c;
            return this;
        }

        // Token: 0x04000016 RID: 22
        public LineRenderer lineRenderer;

        // Token: 0x04000017 RID: 23
        public Color color = Color.red;
    }
}
