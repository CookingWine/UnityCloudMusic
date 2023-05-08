using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CloudMusic
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style" , "IDE0090:使用 \"new(...)\"" , Justification = "<挂起>" )]
    public class CloudMusicHttp :MonoBehaviour
    {
        ///<summary>创建一个默认请求</summary>
        public void CreateCloudRequet( string url , Action<DownloadHandler> successCallback , Action<string> failedCallback )
        {
            StartCoroutine( HttpRequetData( url , 5 , successCallback , failedCallback ) );
        }
        ///<summary>创建一个<seealso cref="Texture2D"/>请求</summary>
        public void CreateCloudRequet( string url , Action<Texture2D> successCallback = null , Action<string> failedCallback = null )
        {
            StartCoroutine( HttpRequetTexture( url , successCallback , failedCallback ) );
        }
        ///<summary>创建一个<seealso cref="AudioClip"/>请求</summary>
        public void CreateCloudRequet( string url , Action<AudioClip> successCallback = null , Action<string> failedCallback = null )
        {
            StartCoroutine( HttpRequetMusic( url , successCallback , failedCallback , AudioType.MPEG ) );
        }
        public void CreateCloudRequetM4A( string url , Action<AudioClip> successCallback = null , Action<string> failedCallback = null )
        {
            StartCoroutine( HttpRequetMusicM4A( url , successCallback , failedCallback ) );
        }
        private IEnumerator HttpRequetData( string url , int awaitTime , Action<DownloadHandler> successCallback , Action<string> failedCallback )
        {
            using UnityWebRequest request = new UnityWebRequest( url );
            request.timeout = awaitTime;
            DownloadHandlerBuffer data = new DownloadHandlerBuffer( );
            request.downloadHandler = data;
            yield return request.SendWebRequest( );
            if( request.result == UnityWebRequest.Result.Success )
            {
                successCallback?.Invoke( request.downloadHandler );
            }
            else
            {
                failedCallback?.Invoke( request.error );
            }
        }

        private IEnumerator HttpRequetTexture( string url , Action<Texture2D> successCallback , Action<string> failedCallback )
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture( url );
            yield return request.SendWebRequest( );
            if( request.result == UnityWebRequest.Result.Success )
            {
                Texture2D texture = DownloadHandlerTexture.GetContent( request );
                successCallback?.Invoke( texture );
            }
            else
            {
                failedCallback.Invoke( request.error );
            }
        }

        private IEnumerator HttpRequetMusic( string url , Action<AudioClip> successCallback , Action<string> failedCallback , AudioType type = AudioType.MPEG )
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip( url , type );
            yield return request.SendWebRequest( );
            if( request.result == UnityWebRequest.Result.Success )
            {
                successCallback?.Invoke( DownloadHandlerAudioClip.GetContent( request ) );
            }
            else
            {
                failedCallback.Invoke( request.error );
            }
        }

        private IEnumerator HttpRequetMusicM4A( string url , Action<AudioClip> successCallback , Action<string> failedCallback )
        {
            WWW request = new WWW( url );
            yield return request;
            if( !string.IsNullOrEmpty( request.error ) )
            {
                Debug.LogError( "下载失败" );
                failedCallback?.Invoke( request.error );
                yield break;
            }
            AudioClip clip = request.GetAudioClip( false , true , AudioType.MPEG );
            while( clip.loadState != AudioDataLoadState.Loaded )
            {
                yield return null;
            }
            successCallback?.Invoke( clip );
        }
    }
}