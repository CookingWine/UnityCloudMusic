using UnityEngine;

namespace CloudMusic
{
    public class GameCloudMusic :MonoBehaviour
    {
        public static GameCloudMusic Instance;

        public CloudMusicHttp CloudRequest { get; private set; }
        public AudioSource source;
        private void Awake( )
        {
            Instance = this;
            CloudRequest = GetComponent<CloudMusicHttp>( );
        }
        private void Start( )
        {
            //CloudRequest.CreateCloudRequetM4A( "https://dl.stream.qqmusic.qq.com/C400001WYACn4XVhRT.m4a?guid=4284598090&vkey=62ACC8FFF2197493A4BE4B40FD225ED354FB86D7D2FB8B206892D2CF3A8A312B892E754B7388CC40309EF696EBB6A2E97106536D8B555E5A&uin=3181983989&fromtag=120032" , (data ) =>
            //{
            //    source.clip= data;
            //    source.Play();
            //} );
        }
    }
}