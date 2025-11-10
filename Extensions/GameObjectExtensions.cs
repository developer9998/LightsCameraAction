using UnityEngine;

namespace LightsCameraAction.Extensions
{
    // Token: 0x02000006 RID: 6
    public static class GameObjectExtensions
    {
        // Token: 0x06000014 RID: 20 RVA: 0x00002900 File Offset: 0x00000B00
        public static void Obliterate(this GameObject self)
        {
            Object.Destroy(self);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x0000290A File Offset: 0x00000B0A
        public static void Obliterate(this Component self)
        {
            Object.Destroy(self);
        }
    }
}
