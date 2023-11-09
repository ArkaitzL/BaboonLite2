using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

namespace BaboOnLite
{
    public class Idiomas : EditorWindow
    {
        //VARIABLES 

        //Publicas
        [SerializeField] private List<Lenguaje> lenguajesLocal = new();
        [SerializeField] private int actualLocal;

        //Estaticas
        [SerializeField] private static List<Lenguaje> lenguajes = new();

        //Eventos
        public static event Action actualizar;
        public static event Action cambiarTextos;

        //Privadas
        private SerializedObject serializedObject;
        private Vector2 scroll1 = Vector2.zero;
        private List<string> listas = new();
        private bool play;

        [MenuItem("Window/BaboOnLite/Idiomas")]
        public static void IniciarVentana()
        {
            Idiomas ventana = GetWindow<Idiomas>("Idiomas");
            ventana.minSize = new Vector2(200, 200);

           //Dependencia
           Save dependecia = GetWindow<Save>("Save");
           dependecia.minSize = new Vector2(200, 200);
        }

        private void OnGUI()
        {

            //Crea la GUI basica de la ventana

            //Inicio del GUI
            GUILayout.Label("Idiomas: Administra los idiomas de tu juego facilmente", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            //Idioma actual
            #region idioma actual

            int actualAnterior = actualLocal;

            actualLocal = EditorGUILayout.IntSlider("Valor Entero", actualLocal, 0, lenguajesLocal.Count-1);

            if (actualAnterior != actualLocal)
            {
                Save.Data.lenguaje = actualLocal;
                cambiarTextos?.Invoke();
            }

            EditorGUILayout.Space(10);

            #endregion

            Separador();

            //Los diccionarios de los lenguajes
            #region diccionarios

            //Boton de actualizar
            if (GUILayout.Button("Actualizar diccionarios"))
            {
                Actualizar();
            }

            //Imprime la lista de diccionarios
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lenguajesLocal"));

            if (serializedObject.ApplyModifiedProperties()) {
                Actualizar();
            }

            //Imprimir los datos en labels
            GUILayout.Label("Mis diccionarios:", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scroll1 = EditorGUILayout.BeginScrollView(scroll1);

            foreach (var texto in listas)
            {
                EditorGUILayout.LabelField(texto);
            }

            EditorGUILayout.EndScrollView();

            #endregion
        }
        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            Actualizar();

            //Detecta cuando entras en el playmode
            #region play
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
                if (state == PlayModeStateChange.ExitingEditMode) {
                    play = true;
                }
            };

            if (play)
            {
                //Evento 
                actualizar += () =>
                {
                    actualLocal = Save.Data.lenguaje;
                    Repaint();
                };
                //Asignar variables
                lenguajes = lenguajesLocal;
            }
            #endregion
        }

        //PUBLICAS

        //Funciones para cambiar el idioma actual
        #region idioma actual
        public static void Alternar()
        {
            Save.Data.lenguaje = (Save.Data.lenguaje < (lenguajes.Count-1))
               ? ++Save.Data.lenguaje
               : 0;
            actualizar?.Invoke();
            cambiarTextos?.Invoke();
        }
        public static void Cambiar(int i)
        {
            //Valida la longitud de miLang
            int longitud = lenguajes.Count-1;

            if (i >= longitud || i < 0)
            {
                //No hay elementos en esa posicion del array
                Bug.LogLite($"[BL][Idiomas: 2]No existe un elemento asignado, a la posicion {i} en tus lenguajes");
                return;
            }

            Save.Data.lenguaje = i;
            actualizar?.Invoke();
            cambiarTextos?.Invoke();
        }
        #endregion

        public static string Get(int i) => lenguajes[Save.Data.lenguaje].dictionary[i];

        //PRIVADAS

        private void Separador(int altura = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, altura);
            rect.height = altura;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private void Actualizar() {
            #region actualizar lista de lenguajes

            if (lenguajesLocal.Count == 0) listas.Clear();

            //Cuando modificas la listya comprueba que todo esta bien
            if (lenguajesLocal.Count == 0) return;

            listas.Clear();
            int? comparacion = null;
            foreach (Lenguaje lenguaje in lenguajesLocal)
            {
                if (lenguaje == null) return;

                if (comparacion == null) comparacion = lenguaje.dictionary.Length;
                if (comparacion != lenguaje.dictionary.Length)
                {
                    Bug.LogLite("[BL][Idiomas: 1] Los diccionarios tienen longitudes diferentes");
                    return;
                }
            }

            //Guarda los diccionarios ordenados
            for (int i = 0; i < comparacion; i++)
            {
                listas.Add($"Palabra ->  {i}:");

                for (int j = 0; j < lenguajesLocal.Count; j++)
                {
                    listas.Add($"\t �  {lenguajesLocal[j].name}: " + lenguajesLocal[j].dictionary[i]);
                }
                //listas.Add("\n");
            }

            //Pasa al estatico
            lenguajes = lenguajesLocal;
            #endregion
        }
    }
}
