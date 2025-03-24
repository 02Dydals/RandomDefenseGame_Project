using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

// ���� ��Ī

public partial class Net_Mng : MonoBehaviour
{
    // Lobby -> �÷��̾ ���ϴ� ������ ã�ų�, �� ������ ����� ����ϴ� ��
    // Relay -> ��Ī�� �÷��̾���� Relay�� Join Code�� ����Ǿ�, ȣ��Ʈ-Ŭ���̾�Ʈ ������� �ǽð� ��Ƽ�÷��� ȯ���� ����
    private Lobby currentLobby;
    public Button StartMatchButton;
    //public Button JoinMatchButton;
    //public Text JoinCodeText;
    //public InputField fieldText;

    public GameObject Matching_Object;
    public Button CancelButton;

    // ���� �̵��ϴ� ����
    private const int maxPlayers = 2;
    private string gameplaySceneName = "GamePlayScene";

    private async void Start() // �񵿱� : ���ÿ� �Ͼ�� �ʴ´�.
    {
        await UnityServices.InitializeAsync();// �ʱ�ȭ
        if(!AuthenticationService.Instance.IsSignedIn) // �α����� �Ǿ����� �ʴٸ�
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();// ����Ƽ�� �ٽ� �α���
        }

        StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        //JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(fieldText.text));
    }    
}
