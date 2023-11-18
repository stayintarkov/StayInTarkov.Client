
<div align=center style="text-align: center">
<h1 style="text-align: center"> SIT.Core </h1>
Ein Escape From Tarkov BepInEx Modul für SPT-Aki mit dem Ziel einen "Offline" Coop-Modus zu emulieren.
</div>

---

<div align=center>

![GitHub all releases](https://img.shields.io/github/downloads/paulov-t/SIT.Core/total) ![GitHub release (latest by date)](https://img.shields.io/github/downloads/paulov-t/SIT.Core/latest/total)

[English](README.md) **|** [Deutsch](README_DE.md) **|** [Português-Brasil](README_PO.md) **|** [简体中文](README_CN.md)

</div>

---

## Über

Das Projekt wurde ins Leben gerufen da Battlestate Games (BSG) bisher keine reine PvE-Erfahrung mit gespeichertem Fortschritt bietet. Das Ziel ist einfach: Ein PvE Spielmodus in dem du mit Freunden spielen kannst und deine/eure Items und Fortschritt erhalten bleiben.
Sobald BSG allerdings solch einen Spielmodus anbietet wird das Projekt sofort eingestampft!

## Haftungsausschluss

* Du brauchst eine legitime Version von Escape from Tarkov. Kaufen kannst du es hier: [https://www.escapefromtarkov.com](https://www.escapefromtarkov.com). 
* Das Projekt stellt keine Grundlage zum cheaten dar! (Es wurde unter anderem ins Leben gerufen da Cheater die Live-Server verpesten)
* Auch ist dies keine Grundlage zum cracken, es sind entsprechende Checks implementiert, die das verhindern.
* Das Ganze ist nur zu Lernzwecken (Es dient dem lernen von Unity, Reverse-Engineering und Netzwerktechnik)
* Das Projekt hat keine Verbindung zu BSG (Battlestate Games) oder anderen (Reddit, Discord etc.), die behaupten daran zu arbeiten.

## Support

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/N4N2IQ7YJ)
* Der Ko-Fi Link ist wie einen Kaffee kaufen, nicht mehr und nicht weniger.
* Es gibt keinen unveröffentlichten Code der irgendetwas hier ermöglicht, alles ist in der Repo.
* Bitte schenkt kein Geld in der Hoffnung auf Hilfe oder einer Lösung!
* Das ist ein Hobbyprojekt, des Spaßes halber. Nehmt es nicht zu ernst.
* Das Projekt geht weder gegen BSG noch die Tarkov-Community!
* Pull-Requests sind gerne gesehen!

## SPT-AKI Voraussetzung
* Stay in Tarkov benötigt den neusten [AKI Server](https://dev.sp-tarkov.com/SPT-AKI/Server) um zu funktionieren. Du kannst mehr über SPT-AKI [hier](https://www.sp-tarkov.com/) erfahren.
* NICHT DEN KLIENTEN! DU BRAUCHST DEN SERVER!

## [Wiki](https://github.com/paulov-t/SIT.Core/wiki)
**Das Wiki wird von mehreren Autoren verwaltet und ist möglicherweise nicht immer ganz aktuell!**
  - ### [Setup](https://github.com/paulov-t/SIT.Core/wiki/Home-Deutsch)
  - ### [FAQs](https://github.com/paulov-t/SIT.Core/wiki/FAQs-Deutsch)

## Coop

### Zusammenfassung
**ACHTUNG**
* Coop ist im Anfangsstadium. 
* Die meisten Sachen funktionieren einigermaßen und im Grunde ist es "spielbar". Verwechsel hier aber spielbar nicht mit perfekt, erwarte Bugs, Lag, Desync und anderes!
* Alle Karten wurden getestet. Factory und Labs laufen am flüssigsten. Performance ist trotzdem abhängig von deinem CPU und der Verbindung zwischen dir und dem Server.
* Mehr Infos zum Hosten findest du [hier](https://github.com/paulov-t/SIT.Core/wiki/Home-Deutsch).

### Voraussetzung
Du brauchst den [SIT-Servermod](https://github.com/paulov-t/SIT.Aki-Server-Mod) und musst ihn auf deinem SPT-AKI Server installieren.

### Nutzt der Coop Netzwerkcode von BSG?
Nein, alles Serverseitige ist aus diversen Gründen für Klienten nicht einseh- und deshalb nicht nutzbar.

### Code-Erklärung
- Das Projekt baut auf diversen BepInEx Harmony Patches im Zusammenspiel mit Unity-Komponenten auf.
- Features/Methoden erfordern konstanten austausch zwischen Clienten und Server (Bewegung, Rotation usw.) und nutzen angehängte Komponenten um Daten zu senden und empfangen.
- Features/Methoden die einfach replitziert werden können nutzen eine abstrakte `ModuleReplicationPatch` Klasse zum senden/empfangen.
- Sämtliche Kommunikation geschiet via JSON TCP Http und WebSockets zum ["Web Server" von SPT-Aki](https://dev.sp-tarkov.com/SPT-AKI/Server). Der Servermod ist in Typescript und handhabt das "backend".
- Ein `CoopGameComponent` wird in die Spielweld geladen sobald ein Coop-Fähiges spielt gestartet wurde (alles ausser deinem Hideout), kümmert sich um die Serverkommunikation und gibt alles an das `PlayerReplicatedComponent` weiter.

## SPT-Aki

### Werden Aki-Module unterstützt?
Die folgenden Aki-Module werden unterstützt:
- aki-core
- Aki.Common
- Aki.Reflection
- 50/50 was SPT-AKI Client mods angeht. Ist abhängig davon wie gut das Modul geschrieben wurde. Wenn diese direkt GCLASSXXX Klassen nutzen werden sie wahrscheinlich nicht funktionieren.

### Warum nutzt du nicht Aki-Modul DLLs?
SPT-AKI DLL's sind speziell für deren Deobfuscationstechnik geschrieben und sind teilweise inkompatibel mit meiner eigenen Technik.
Es wurden bereits viele Features portiert aber am Ende soll das Projekt unabhängig laufen können.

## Wie kann ich das Projekt selbst kompilieren? 
[Compiling Document](COMPILE.md)

# Wie installiere ich BepInEx?
[https://docs.bepinex.dev/articles/user_guide/installation/index.html](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

## In Tarkov installieren
BepInEx 5 muss erst installiert und konfiguriert werden.
Platziere die kompilierte .dll in deinen BepInEx plugin Ordner.

## In Tarkov testen
- Öffne deine BepInEx config
- Ändere [Logging.Console] von Enabled to True
- Speichern
- Starte Tarkov über den SIT-Launcher oder eine Batch-Datei z.B.:
```
start ./Clients/EmuTarkov/EscapeFromTarkov.exe -token=pmc062158106353313252 -config={"BackendUrl":"http://127.0.0.1:6969","Version":"live"}
```
- 
- Wenn du alles richtig gemacht hast sollte ein BepInEx Konsolenfenster aufgehen und anzeigen, dass das Plugin geladen/gestartet wurde.


## Danksagung
- SPT-Aki team
- MTGA team
- SPT-Aki Modding Community
- Props (AIBushPatch, AIAwakeOrSleepPatch - Currently unused)

## Lizenz

- 95% des Original-Codes und die gesammte Einzelspieler-Funktionalität wurde vom SPT-AKI team geschrieben. Möglicherweise fallen Teile unter deren Lizenz.
- Nichts hier ist lizensiert, das ist ein reines Spaßprojekt. Nutz den Code wofür du willst.
