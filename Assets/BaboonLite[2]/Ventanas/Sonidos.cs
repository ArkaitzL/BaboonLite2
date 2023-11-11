using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BaboOnLite
{
    public class Sonidos : EditorWindow
    {
        //VARIABLES 

        //Publicas
        [SerializeField] private DictionaryBG<AudioClip> sonidosLocal = new();
        [SerializeField] private DictionaryBG<AudioClip> musicaLocal = new();

        //Estaticas
        [SerializeField] private static DictionaryBG<AudioClip> sonidos = new();
        [SerializeField] private static DictionaryBG<AudioClip> musica = new();

        //Privadas
        private bool autoPlay, play;
        private Vector2 scroll = Vector2.zero;
        private SerializedObject serializedObject;

        //Referencias
        public Dictionary<string, Sonido> sonido { get => Save.Data.sonido; set => Save.Data.sonido = value; }

        [MenuItem("Window/BaboOnLite/Sonidos")]
        public static void IniciarVentana()
        {
            Sonidos ventana = GetWindow<Sonidos>("Sonidos");
            ventana.minSize = new Vector2(200, 200);

            //Dependencia
            Save dependecia = GetWindow<Save>("Save");
            dependecia.minSize = new Vector2(200, 200);
        }

        private void OnGUI()
        {
            //Crea la GUI basica de la ventana

            //Inicio del GUI
            GUILayout.Label("Sonidos: Administra los sonidos de tu juego facilmente", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);

            //Administra el volumen y el estado de la vibracion, musica y sonidos
            #region sonidos volumen/estado
            EditorGUILayout.Space(10);

            GUILayout.Label("Vibracion: ", EditorStyles.boldLabel);
            sonido["vibracion"].estado = EditorGUILayout.Toggle("Activo: ", sonido["vibracion"].estado);

            EditorGUILayout.Space(10);

            GUILayout.Label("Sonidos: ", EditorStyles.boldLabel);
            sonido["sonidos"].estado = EditorGUILayout.Toggle("Activo: ", sonido["sonidos"].estado);
            sonido["sonidos"].volumen = EditorGUILayout.IntSlider("Volumen: ", sonido["sonidos"].volumen, 0, 100);

            EditorGUILayout.Space(10);

            GUILayout.Label("Musica: ", EditorStyles.boldLabel);
            sonido["musica"].estado = EditorGUILayout.Toggle("Activo: ", sonido["musica"].estado);
            sonido["musica"].volumen = EditorGUILayout.IntSlider("Volumen: ", sonido["musica"].volumen, 0, 100);

            EditorGUILayout.Space(10);
            #endregion

            Separador();
            EditorGUILayout.Space(10);

            //Lista de musicas
            #region sonidos
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sonidosLocal"));
            if (serializedObject.ApplyModifiedProperties())
            {
                sonidos = sonidosLocal;
            }
            EditorGUILayout.Space(10);
            #endregion

            Separador();

            //Lista de sonidos
            #region musica
            EditorGUILayout.Space(10);
            autoPlay = EditorGUILayout.Toggle("Reproductor automatico: ", autoPlay);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("musicaLocal"));
            if (serializedObject.ApplyModifiedProperties())
            {
                musica = musicaLocal;
            }
            #endregion

            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            //Declarar el serializedObject
            serializedObject = new SerializedObject(this);

            //Detecta cuando entras en el playmode
            #region play
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingEditMode)
                {
                    play = true;
                }
            };

            if (play)
            {
                //Asigna las variables
                sonidos = sonidosLocal;
                musica = musicaLocal;

                //Reproduce automaticamente la musica
            }
            #endregion
        }

        //Metodos para cambiar el volumen y los estados
        #region sonidos volumen/estado
        public static void Estado(string nombre, bool estado)
        {
            Save.Data.sonido[nombre].estado = estado;
        }
        public static void Volumen(string nombre, int volumen)
        {
            if (volumen < 0)
            {
                volumen = 0;
                Bug.LogLite("[BL][Sonidos: 1] El volumen no puede ser menor que 0");
            }
            else if (volumen < 0)
            {
                volumen = 100;
                Bug.LogLite("[BL][Sonidos: 2] El volumen no puede ser mayor que 100");
            }
            Save.Data.sonido[nombre].volumen = volumen;
        }
        #endregion

        //Metodos para instanciar un sonido o vibracion
        #region instanciar
        public static void GetVibracion()
        {
            if (Save.Data.sonido["vibracion"].estado)
            {
                if (SystemInfo.supportsVibration)
                {
                    Handheld.Vibrate();
                    return;
                }
                Debug.Log("El dispositivo actual no soporta la vibracion");
            }
        }
        public static AudioSource GetSonido(string nombre, bool inmortal = false, bool bucle = false)
        {
            if (Save.Data.sonido["sonidos"].estado)
            {
                return Creador(sonidos, nombre, Save.Data.sonido["sonidos"].volumen, inmortal, bucle);
            }
            return new();
        }
        public static AudioSource GetMusica(string nombre, bool inmortal = false, bool bucle = false)
        {
            if (Save.Data.sonido["musica"].estado)
            {
                return Creador(musica, nombre, Save.Data.sonido["musica"].volumen, inmortal, bucle);
            }
            return new();
        }
        private static AudioSource Creador(DictionaryBG<AudioClip> lista, string nombre, int volumen, bool inmortal, bool bucle)
        {
            //Comprueba que exista el audio
            if (!lista.Inside(nombre))
            {
                //Ese sonido no esta dentro del array
                Bug.LogLite($"[BL][Sonidos: 3] No existe el sonido {nombre} dentro de Sounds");
                return null;
            }

            //Instancia el audio
            GameObject instancia = new GameObject($"Sound-{nombre}");
            instancia.transform.position = Vector3.zero;
            //instancia.transform.SetParent(padre);

            //Crea el audioi source
            AudioSource audioSource = instancia.AddComponent<AudioSource>();
            audioSource.clip = lista.Get(nombre);
            audioSource.volume = ((float)volumen / 100).Log() ;

            //Le da la inmortalidad entre escena
            if (inmortal) DontDestroyOnLoad(instancia);

            //Lo activa en bucle
            if (bucle) audioSource.loop = true;
            else Destroy(instancia, lista.Get(nombre).length);

            //Lo activa y devuelve
            audioSource.Play();
            return audioSource;
        }
        #endregion

        //Metodos para a�adir dise�os al gui
        #region gui dise�o
        private void Separador(int altura = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, altura);
            rect.height = altura;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
        #endregion
    }
}