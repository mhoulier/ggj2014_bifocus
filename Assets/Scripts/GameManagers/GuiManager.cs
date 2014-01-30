using UnityEngine;
using System.Collections;

public class GuiManager : MonoBehaviour
{
	static public void GUIDrawTextureOnScreen(Texture2D _Texture)
	{
		float texHeight = _Texture.height;
		float texWidth = _Texture.width;
		float texRatio = texWidth / texHeight;
		
		float screenWidth = Screen.width;
		float screenHeight = Screen.height;
		float screenRatio = screenWidth / screenHeight;
		
		float texScreenOffsetX = 0.0f;
		float texScreenOffsetY = 0.0f;
		float texScreenWidth = screenWidth;
		float texScreenHeight = screenHeight;
		
		if (texRatio >= screenRatio)
		{
			texScreenHeight = texScreenWidth / texRatio;
			texScreenOffsetY = (screenHeight - texScreenHeight) / 2;
		}
		else
		{
			texScreenWidth = texScreenHeight * texRatio;
			texScreenOffsetX = (screenWidth - texScreenWidth) / 2;
		}
		
		GUI.DrawTexture(new Rect(texScreenOffsetX, texScreenOffsetY, texScreenWidth, texScreenHeight), _Texture);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
