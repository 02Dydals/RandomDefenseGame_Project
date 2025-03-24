using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;

public partial class Net_Mng : MonoBehaviour
{
    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrEmpty(inputJoinCode))
        {
            Debug.Log("유효하지 않은 Join Code입니다.");
            return;
        }
        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);
            // 호스트가 가지고 있는 데이터에 연결하는 메서드
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            StartClient();
            Debug.Log("Join Code로 게임에 접속 성공!");
        }

        catch (RelayServiceException e)
        {
            Debug.Log("게임 접속 실패 :" + e);
        }
    }

    // 버튼에 연결, 로비를 만들지 접근할지 확인하는 함수
    public async void StartMatchmaking()
    {
        if (!AuthenticationService.Instance.IsSignedIn) // 로그인이 되어있지 않다면
        {
            Debug.Log("로그인되지 않았습니다");
            return;
        }
        // 
        Matching_Object.SetActive(true);
        currentLobby = await FindAvailableLobby();        
        if (currentLobby == null)// 로비가 없다면 로비를 만든다
        {
            await CreateNewLobby(); // 호스트가 되어 로비를 만든다.
        }

        else
        {
            await JoinLobby(currentLobby.Id);// 로비에 접근한다
        }        
    }

    // 로비를 찾는 함수
    private async Task<Lobby> FindAvailableLobby()
    {
        // 예외 처리
        try
        {
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            if (queryResponse.Results.Count > 0)
            {
                return queryResponse.Results[0]; // 가장 처음으로 만들어진 로비
            }
        }

        catch (LobbyServiceException e)
        {
            Debug.Log("로비 찾기 실패" + e);
        }
        return null;
    }

    private async void DestroyLobby(string lobbyId)
    {
        try
        {
            if(!string.IsNullOrEmpty(lobbyId))
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                Debug.Log("Lobby Destroyed : " + lobbyId);
            }

            if(NetworkManager.Singleton.IsHost) // 파괴한 주체가 호스트라면
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        catch(System.Exception e)
        {
            Debug.LogError("Failed to destroy lobby" + e.Message);
            Matching_Object.SetActive(false);
        }
    }

    // 방을 만든다 -> 호스트가 
    private async Task CreateNewLobby()
    {
        try
        {
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("랜덤매칭방", maxPlayers);
            Debug.Log("새로운 방 생성됨" + currentLobby.Id);
            await AllocateRelayServerAndJoin(currentLobby);
            CancelButton.onClick.AddListener(() => DestroyLobby(currentLobby.Id));
            StartHost();
        }

        catch (LobbyServiceException e)
        {
            Debug.Log("로비 찾기 실패" + e);
        }
    }

    // 클라이어트가 되는 조건
    private async Task JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log("방에 접속되었습니다." + currentLobby.Id);
            StartClient();
        }

        catch (LobbyServiceException e)
        {
            Debug.Log("로비 참가 실패 : " + e);
        }
    }


    // 서버에 로비를 만드는 함수
    private async Task AllocateRelayServerAndJoin(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            //JoinCodeText.text = joinCode;
            Debug.Log("Relay 서버 할당 완료. Join Code :" + joinCode);
        }

        catch (RelayServiceException e)
        {
            Debug.Log("Relay 서버 할당 실패 : " + e);
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트가 시작되었습니다.");

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnHostDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        OnPlayerJoined();
    }

    private void OnHostDisconnected(ulong clientId)
    {
        if(clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostDisconnected;
        }
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트가 연결되었습니다.");
    }
}
