using SpellFlinger.Enum;
using SpellFlinger.Scriptables;
using SpellSlinger.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpellFlinger.LoginScene
{
    public class RoomCreationView : MonoBehaviour
    {
        [SerializeField] private Toggle _teamDeathMatchToggle = null;
        [SerializeField] private Toggle _deathMatchToggle = null;
        [SerializeField] private TMP_InputField _roomNameInput = null;
        [SerializeField] private Button _returnButton = null;
        [SerializeField] private Button _createRoomButton = null;
        [SerializeField] private LevelSelectionToggle _levelSelectionTogglePrefab = null;
        [SerializeField] private ToggleGroup _levelSelectionContainer = null;
        [SerializeField] private GameObject _sessionView = null;
        private LevelType _selectedLevelType;

        private void Awake()
        {
            /*
             * Metodu je potrebno nadopuniti s kodom za stvaranje Toggle objekata za izbor scene.
             * Popis podataka o scenama se moze dobiti iz instance LevelDataScriptable klase. +

             * Stvaranje i inicijalizaciju objekata se provodi na slican nacin kao stvaranje
             * Toggle objekata za izbor oruzja u Awake metodi SessionView klase. +
             * 
             * Nakon toga je potrebno inicijalizirati callback Return gumba da na pritisak gasi 
             * izbornik stvaranja sobe i otvara izbornik odabira otvorenih soba.+

             * Takoder potrebno je inicijalizirati callback Create Room gumba da na pritisak
             * poziva metodu za stvaranje sobe.+
             */

        
            foreach (var level in LevelDataScriptable.Instance.Levels)
            {
                LevelSelectionToggle levelToggle = Instantiate(_levelSelectionTogglePrefab, _levelSelectionContainer.transform);
                levelToggle.ShowLevel(level.LevelType, _levelSelectionContainer, level.LevelImage, (levelType) => _selectedLevelType=levelType);
            }

            _returnButton.onClick.AddListener(() =>
            {
                _sessionView.SetActive(true);
                gameObject.SetActive(false);
            });
            _createRoomButton.onClick.AddListener(() => CreateRoom());

        }

        private void CreateRoom()
        {
            /*  
             *  Potrebno je pozvati metodu instance FusionConnection za stvaranje sobe, te joj poslati potrebne parametre.
             */
            GameModeType _selectedGameMode;
            if (_teamDeathMatchToggle.isOn){
                _selectedGameMode = GameModeType.TDM;
            }else {
                _selectedGameMode = GameModeType.DM;
            }
             FusionConnection.Instance.CreateSession(_roomNameInput.text,  _selectedGameMode,  _selectedLevelType);
        }
    }
}
