using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// This class gets 3D textures decoded from the media player, splits them into each eye textures, and
/// renders on the overlay geometry.
/// </summary>
public class TopBottomFrameTransmitter : MonoBehaviour
{
    /// <summary>
    /// Triple buffer the render target
    /// </summary>
    private const int overlayRTChainSize = 3;
#if (UNITY_ANDROID) && (!UNITY_EDITOR)
	private int overlayRTIndex = 0;
#endif

    private IntPtr[] overlayTexturePtrs = new IntPtr[overlayRTChainSize * 2];
    private RenderTexture[] overlayRTChain = new RenderTexture[overlayRTChainSize * 2];

    private int textureWidth = 0;
    private int textureHeight = 0;
    private RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32;

    private Material LeftEyeMaterial = null;
    private Material RightEyeMaterial = null;

    /// <summary>
    /// The Media player to prvovide 3D textures
    /// </summary>
    public JackalopeMediaPlayer m_SourceMediaPlayer = null;

    /// <summary>
    /// Destination OVROverlay target object
    /// </summary>
    public GameObject m_TargetOverlayScreen;

    /// <summary>
    /// Triple buffer the textures applying to overlay
    /// </summary>
    void ConstructRenderTextureChain()
    {
        for (int i = 0; i < overlayRTChainSize * 2; i++)
        {
            overlayRTChain[i] = new RenderTexture(textureWidth + 2, textureHeight + 2, 1, textureFormat, RenderTextureReadWrite.sRGB);
            overlayRTChain[i].antiAliasing = 1;
            overlayRTChain[i].depth = 0;
            overlayRTChain[i].wrapMode = TextureWrapMode.Clamp;
            overlayRTChain[i].hideFlags = HideFlags.HideAndDontSave;
            overlayRTChain[i].Create();
            overlayTexturePtrs[i] = overlayRTChain[i].GetNativeTexturePtr();
        }
    }

    void ConstructEyeMaterials()
    {
        Shader shader = Shader.Find("VOKE/Split 3D Texture");
		//Shader shader = Shader.Find ("VOKE/Split 3D Texture-SoftEdge");
        if (shader == null)
            Debug.LogError("Could not find the shader");

        LeftEyeMaterial = new Material(shader);
        LeftEyeMaterial.SetVector("_Offset", new Vector4(0, 0.5f, 1f, 0.5f));
        LeftEyeMaterial.SetVector("_ImageSize", new Vector4(textureWidth, textureHeight, 0, 0));
		//LeftEyeMaterial.SetTexture("_AlphaMap", Resources.Load("ScreenBlurredEdges") as Texture);

        RightEyeMaterial = new Material(shader);
        RightEyeMaterial.SetVector("_Offset", new Vector4(0, 0f, 1f, 0.5f));
        RightEyeMaterial.SetVector("_ImageSize", new Vector4(textureWidth, textureHeight, 0, 0));
		//RightEyeMaterial.SetTexture("_AlphaMap", Resources.Load("ScreenBlurredEdges") as Texture);

    }

    void Start()
    {
        // Check the input variables
        if (m_SourceMediaPlayer == null || m_TargetOverlayScreen == null)
        {
            Debug.LogError("TopBottomFrameTransmitter::The source or target object is not specified!!!");
            return;
        }

        textureWidth = m_SourceMediaPlayer.TextureWidth;
        textureHeight = m_SourceMediaPlayer.TextureHeight / 2;
        if (textureWidth <= 0 || textureHeight <= 0)
        {
            Debug.LogError("TopBottomFrameTransmitter::The texture dimension is not specified!!!");
            return;
        }

#if (UNITY_ANDROID) && (!UNITY_EDITOR)
			ConstructRenderTextureChain();

			ConstructEyeMaterials();
#endif
    }

    /// <summary>
    /// Copy camera's render target to triple buffered texture array and send it to OVROverlay object
    /// </summary>
    void Update()
    {
        // Check the input variables
        if (m_SourceMediaPlayer == null || m_TargetOverlayScreen == null ||
            LeftEyeMaterial == null || RightEyeMaterial == null)
        {
            return;
        }

#if (UNITY_ANDROID) && (!UNITY_EDITOR)
		// Get a 3D texture decoded fromt the media player
		Texture2D texSource = m_SourceMediaPlayer.GetNativeTexure ();
        if (texSource == null)
            return;

		// Split the 3D texture into textures for each eyes
		Graphics.Blit(texSource, overlayRTChain[overlayRTIndex], LeftEyeMaterial);
		Graphics.Blit(texSource, overlayRTChain[overlayRTIndex + 1], RightEyeMaterial);

		// Render the each eye textures onto the overlay geometry
		OVROverlay ovrOverlay = m_TargetOverlayScreen.GetComponent<OVROverlay>();
		Debug.Assert(ovrOverlay);

		ovrOverlay.OverrideOverlayTextureInfo(overlayRTChain[overlayRTIndex], overlayTexturePtrs[overlayRTIndex], UnityEngine.XR.XRNode.LeftEye);
		ovrOverlay.OverrideOverlayTextureInfo(overlayRTChain[overlayRTIndex + 1], overlayTexturePtrs[overlayRTIndex + 1], UnityEngine.XR.XRNode.RightEye);
		overlayRTIndex += 2;
		overlayRTIndex = overlayRTIndex % (overlayRTChainSize * 2);
#endif
    }
}
