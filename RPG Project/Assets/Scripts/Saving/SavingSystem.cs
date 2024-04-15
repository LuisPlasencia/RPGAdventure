using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Saving
{
    public class SavingSystem : MonoBehaviour
    {
        public IEnumerator LoadLastScene(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (state.ContainsKey("lastSceneBuildIndex"))
            {
                buildIndex = (int)state["lastSceneBuildIndex"];
            }
            yield return SceneManager.LoadSceneAsync(buildIndex);   // lo que va después de esta línea se ejecuta después de que todos los awake de la nueva scene se hayan llamado. De ahí la necesidad del yield - operaciones asíncronas han de ser esperadas 
            // si pusieramos el loadsceneAsync dentro del if, podría haber race condition porque llamamos a restoreState antes de que los awake de otras clases se hayan llamado y puede que el navmeshagent no este seteado para entonces en Mover. Pudiendo romper la ejecución del juego.
            // para forzar un race condition podemos ir al Script Execution Order en project settings de Unity y hacer que una clase se ejecute antes que otra. (persistentobjectspawner() antes que move() por ejemplo)
            RestoreState(state);
        }

        public void Save(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile);
            CaptureState(state);
            SaveFile(saveFile, state);
        }

        public void Load(string saveFile)
        {
            RestoreState(LoadFile(saveFile));
        }

        public void Delete(string saveFile)
        {
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        private Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path))
            {
                return new Dictionary<string, object>();
            }
            using (FileStream stream = File.Open(path, FileMode.Open))
            {

                // esta forma es manual, sin utilizar las funciones de serialización built-in de C#
                // byte[] buffer = new byte[stream.Length];  // prueba
                // stream.Read(buffer, 0, buffer.Length);

                // // print(Encoding.UTF8.GetString(buffer));

                // Transform playerTransform = GetPlayerTransform();   // prueba 2
                // playerTransform.position = DeserializeVector(buffer);

                // esta es la forma buena
                BinaryFormatter formatter = new BinaryFormatter();
                return (Dictionary<string, object>)formatter.Deserialize(stream);
            }
        }

        private void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Saving to " + path);
            using (FileStream stream = File.Open(path, FileMode.Create))
            {

                // esta forma es manual, sin utilizar las funciones de serialización built-in de C#
                //  //   // stream.WriteByte(0xc2);
                //  //   // stream.WriteByte(0xa1);
                //  //   // stream.Writebyte(0x48);  // estas 3 líneas son !H solamente. Es mucho más fácil hacerlo como abajo. UTF-8 contiene ASCII y más cosas
                //  // byte[] bytes = Encoding.UTF8.GetBytes("¡Hola Mundo!");    // prueba
                //  // stream.Write(bytes, 0, bytes.Length);
                //
                // byte [] buffer = SerializeVector(playerTransform.position);  // prueba 2 
                // stream.Write(buffer, 0, buffer.Length);


                // esta es la forma buena
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }  // el using cierra el stream después de escribir dentro con un Stream.Close(), si no cerramos el stream ocuparía espacio en memoria
            // esto nos evita tener leaks de memoria si ocurrieran excepciones antes de cerrar ya que fuerza el cierre
        }

        private void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                state[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
            }

            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile + ".sav");  // persistentdatapath tiene la ubicación donde podemos escribir: C:\Users\Usuario\AppData\LocalLow\DefaultCompany\RPG Project\
        }


     // OTRA MANERA MÁS BÁSICA Y MANUAL DE AÑADIR LA POSICIÓN AL FICHERO (se pondría muy complicado si quisieramos serializar una serie de objetos enlazados de esta forma)   - prueba 2
     // esta forma es manual, sin utilizar las funciones de serialización built-in de C#
        // private byte[] SerializeVector(Vector3 vector)
        // {
        //     byte [] vectorBytes = new byte[3 * 4];   // necesitamos 3 floats, cada float ocupa 4 bytes
        //     BitConverter.GetBytes(vector.x).CopyTo(vectorBytes, 0);  // convierte a binario - desde el byte 0 al byte 3 del array escribimos el primer float (4 bytes)
        //     BitConverter.GetBytes(vector.y).CopyTo(vectorBytes, 4);
        //     BitConverter.GetBytes(vector.z).CopyTo(vectorBytes, 8);
        //     return vectorBytes;
        // }

        // private Vector3 DeserializeVector(byte[] buffer)   // buffer = memoria donde escribir
        // {
        //     Vector3 result = new Vector3();
        //     result.x = BitConverter.ToSingle(buffer, 0);   // la mitad de un double = single = el tamaño de un float 
        //     result.y = BitConverter.ToSingle(buffer, 4);
        //     result.z = BitConverter.ToSingle(buffer, 8);
        //     return result;
        // }


        // private Transform GetPlayerTransform()
        // {
        //     return GameObject.FindWithTag("Player").transform;
        // }
    }
}