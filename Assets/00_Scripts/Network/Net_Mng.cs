using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

// 랜덤 매칭

public partial class Net_Mng : MonoBehaviour
{
    // Lobby -> 플레이어가 원하는 게임을 찾거나, 새 게임을 만들고 대기하는 방
    // Relay -> 매칭된 플레이어들의 Relay의 Join Code로 연결되어, 호스트-클라이언트 방식으로 실시간 멀티플레이 환경을 유지
    private Lobby currentLobby;
    public Button StartMatchButton;
    //public Button JoinMatchButton;
    //public Text JoinCodeText;
    //public InputField fieldText;

    public GameObject Matching_Object;
    public Button CancelButton;

    // 씬을 이동하는 변수
    private const int maxPlayers = 2;
    private string gameplaySceneName = "GamePlayScene";

    private async void Start() // 비동기 : 동시에 일어나지 않는다.
    {
        await UnityServices.InitializeAsync();// 초기화
        if(!AuthenticationService.Instance.IsSignedIn) // 로그인이 되어있지 않다면
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();// 유니티로 다시 로그인
        }

        StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(fieldText.text));
    }    
}
