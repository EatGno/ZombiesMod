using GTA;
using System.Collections.Generic;

namespace ZombiesMod.Server
{
    public class ZombieSpawner : Script
    {
       private List<Walker> activeWalkers = new List<Walker>();

       public ZombieSpawner()
       {
           Tick += OnTick; // Appelle la méthode OnTick chaque tick du serveur.
       }

       private async void OnTick()
       {
           // Logique pour faire apparaître des zombies régulièrement.
           await Delay(5000); // Attendre 5 secondes entre les apparitions

           Vector3 spawnPosition = Game.Player.Character.Position + new Vector3(10f, 10f, 0f); // Position de spawn relative au joueur
           SpawnWalker(spawnPosition); // Appelle la méthode pour faire apparaître un zombie.
       }

       public void SpawnWalker(Vector3 position)
       {
           int handle = World.CreatePed(ModelHash.WalkerModel, position); // Remplacez par le hash du modèle de zombie que vous souhaitez utiliser.
           Walker walker = new Walker(handle);
           activeWalkers.Add(walker); // Ajoute le walker à la liste active.
       }
    }
}
