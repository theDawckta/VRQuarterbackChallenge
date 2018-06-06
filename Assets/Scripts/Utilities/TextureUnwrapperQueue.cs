using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureUnwrapperQueue : Singleton<TextureUnwrapperQueue>
{
    private Queue _waitingTextureQueue = Queue.Synchronized(new Queue());

	public TextureUnwrapper CreateTextureUnwrapper(WWW loadingTexture, bool mipMaps = true)
    {
        var textureUnwrapper = new TextureUnwrapper(loadingTexture, mipMaps);
        _waitingTextureQueue.Enqueue(textureUnwrapper);
        return textureUnwrapper;
    }

    void Update()
    {
        if (_waitingTextureQueue.Count > 0)
        {
			((TextureUnwrapper)_waitingTextureQueue.Dequeue()).Unwrap();
        }
    }


    public class TextureUnwrapper
    {
        private WWW _loadingTexture;
		private Texture2D _texture;
        private bool _isUnwrapped = false;
		private bool _mipsMapsOn = true;

		public TextureUnwrapper(WWW loadingTexture, bool mipMapsOn)
        {
            _loadingTexture = loadingTexture;
			_mipsMapsOn = mipMapsOn;
        }
        public Texture2D Texture
        {
            get { return _texture; }
        }

        public IEnumerator WaitForUnwrapCompletion()
        {
            while (!_isUnwrapped)
            {
                yield return 0;
            }
        }

        public void Unwrap()
        {
			_texture = new Texture2D (_loadingTexture.texture.width, _loadingTexture.texture.height, TextureFormat.RGB24, _mipsMapsOn);
			_loadingTexture.LoadImageIntoTexture (_texture);
            _loadingTexture = null;
            _isUnwrapped = true;
        }
    }

}