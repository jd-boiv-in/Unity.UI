using System;
using UnityEngine;

namespace JD.UI.Sprites
{
    public class FitScreen : MonoBehaviour
    {
        [HideInInspector] public Camera Camera;
        [HideInInspector] public SpriteRenderer Sprite;

        private void Start()
        {
            // Scale sprite to fit camera
            var scale = Camera.aspect * Camera.orthographicSize * 2.0f / Sprite.sprite.bounds.size.x;
            Sprite.transform.localScale = new Vector2(scale, (1 / Camera.aspect) * scale);
        }

        private void OnValidate()
        {
            Sprite = GetComponent<SpriteRenderer>();
            Camera = Camera.main;
        }
    }
}