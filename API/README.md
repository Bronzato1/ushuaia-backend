Dans ce projet on cible le framework **netcoreapp2.2** donc vérifier à deux endroits:

* Dans `launch.json` 

        "program": "${workspaceFolder}/bin/Debug/netcoreapp2.2/API.dll",
     
* Dans `API.csproj` 

        <TargetFramework>netcoreapp2.2</TargetFramework>

S'assurer que tout est OK en supprimant les répertoires _bin_ et _obj_ et compiler (build) `CTRL`+`SHIFT`+`B` 

Et pour savoir si le .NET Core est installé sur la machine lancer la commande: `dotnet --info`

Pour installer le .NET Core: le télécharger en version Build - SDK - x64

------

dotnet ef migrations add InitialCreate

dotnet ef database update

-------

- suppr du répertoire publish
- dotnet publish API -c Release -o ./publish
- copier/coller le rép App_Data dans le rép publish
- clic-droit sur le répertoire publish et choisir Deploy to web app > ushuaiaApp

