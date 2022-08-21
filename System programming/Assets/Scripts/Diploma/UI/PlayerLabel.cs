using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Characters;

namespace UI
{
    public class PlayerLabel : MonoBehaviour
    {
        #region Methods

        public void DrawLabel(Camera camera)
        {
            if (camera == null) return;
            
            var style = new GUIStyle();

            style.normal.background = Texture2D.redTexture;
            style.normal.textColor  = Color.blue;
            
            for (int i = 0; i < ClientScene.objects.Count; i++)
            {
                var otherClient = ClientScene.objects.ElementAt(i).Value;
                var position = camera.WorldToScreenPoint(otherClient.transform.position);
                var renderer = otherClient.GetComponentInChildren<Renderer>();
                
                if (renderer
                        && renderer.isVisible
                        && otherClient.transform != transform)
                {
                    var labelRect =
                        new Rect(
                            new Vector2(position.x, Screen.height - position.y),
                            new Vector2(10, name.Length * 10.5f));

                    GUI.Label(labelRect, otherClient.name, style);
                };
            };
        }

        #endregion
    }
}
