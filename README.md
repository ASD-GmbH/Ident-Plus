
# Ident-PLUS

Das Programm Ident-PLUS dient dazu, das Öffnen einer Remote-Desktop-Verbindung zu vereinfachen. Hierzu identifiziert sich der Anwender mittels RFID-Chip, woraufhin Ident-PLUS eine RDP-Verbindug zu einem hinterlegten Server öffnet und den Benutzernamen des Anwenders übergibt. Das ggf. für den Login benötigte Passwort wird explizit NICHT von Ident-PLUS gespeichert bzw. übergeben, sondern muss per Hand eingegeben werden.



## Verhalten
* Wird ein Chip auf den Leser aufgelegt, wird eine Remote-Desktop-Verbindung geöffnet.
* Wenn der Chip vom Leser entfernt wird, wird die Verbindung geschlossen.
* Liegt mehr als ein Chip gleichzeitig auf dem Leser, wird eine Fehlermeldung ausgegeben und das Programm gesperrt, bis sämtliche Chips vom Leser entfernt werden. Erst dann kann es wieder normal weiterverwendet werden.

Standardmäßig startet Ident-PLUS nur als Tray-Icon und informiert den Nutzer über Windows-Notifications über aufgelegte oder entfernte Chips usw.  
Falls nötig kann aber auch ein Konsolenfenster aktiviert werden, in dem weitere Informationen zur Verfügung gestellt werden. Um die Konsole anzuzeigen, kann Ident-PLUS mit dem Argument `/k` gestartet werden. Alternativ kann die Konsole im laufenden Betrieb über das Traymenü (Rechtsklick auf das Trayicon) an- bzw. ausgeschaltet werden.

## Konfiguration
Zur Konfiguration wird die `Ident-PLUS.exe.config` im Programmverzeichnis verwendet.
Hier werden die Datenquelle und die RDP-Basis festgelegt.

### Datenquelle
Als Quelle für die Nutzerdaten dient derzeit der P-PLUS-Server. Die entsprechende Ident-PLUS-Schnittstelle ist ab R-369 im Interchange-Modul (Tasks) integriert.  
In der P-PLUS-Datenbank werden die Nutzerdaten in der Tabelle `IdentPLUS_Token` hinterlegt:


...| token          | user             | displayname    | rdp
---| --------       | --------         | --------       | --------
...| `<chipnummer>` | `<rdp-username>` | `<realname>`   | `<rdp-adresse>`
...| 3084763134     | m_mustermann     | Max Mustermann | 192.168.0.123



In der `Ident-PLUS.exe.config` wird Adresse und Port des P-PLUS-Servers hinterlegt:

```xml
<configuration>  
  <connectionStrings>
    <add name="IdentPlusServer" connectionString="tcp://127.0.0.1:21005" />
  </connectionStrings>
</configuration>  
```


### RDP-Basis
Sollen für die RemoteDesktop-Verbindung bestimmte Grundeinstellungen wie z.B. Anzeigegröße, Farbtiefe, Audio- und Tastatureinstellungen verwendet werden, können diese in einer .rdp -Datei hinterlegt werden. Dafür gibt es zwei Möglichkeiten:
1. Als erstes wird überprüft, ob in der `Ident-PLUS.exe.config` eine .rdp-Datei angegeben ist:
```xml
<configuration>
  <appSettings>
    <add key="RDPBasisDatei" value="c:\temp\test.rdp"/>
  </appSettings>
</configuration>  
```
2. Ist ist der value leer (`""`), wird eine ggf. im Ident-PLUS-Verzeichnis vorhandene Datei mit dem Namen `basis.rdp` verwendet.

Wird keine .rdp-Datei vorgefunden, werden keinerlei Voreinstellungen verwendet.

## Installation und Updates
Ident-PLUS verwendet [ClientDeploy](https://github.com/ASD-GmbH/ClientDeploy) zur Installation und zum Update. ClientDeploy besteht aus einer Serverkomponente, einem eigenständigen Installer und einer DLL die von Ident-PLUS verwendet wird.  
Der ClientDeploy-Server muss als erstes eingerichtet werden und stellt die verschiedenen Versionen der Anwendung zur Verfügung. In folgenden Beispielen wird davon ausgegangen, dass der ClientDeploy-Server bereits läuft und Ident-PLUS auf ihm zur Verfügung gestellt wurde.

### Installation
Das ClientDeploy Setup wird wie folgt aufgerufen:  
`setup <repo> <product> <target>`  
`setup http://<ClientDeploy Server Adresse>:<Port>/repo IdentPLUS <Zielverzeichnis>`  
Die Angabe des *products* - hier IdentPLUS - ist case sensitive!

Daraufhin wird die neuste vorliegende Version von Ident-PLUS vom Server heruntergeladen, im Zielverzeichnis installiert und für spätere Updates vorbereitet. Dafür werden das Unterverzeichnis `.clientdeploy` und die Datei `.clientdeploy.config` im Anwendungsverzeinis angelegt.

### Update
Ident-PLUS prüft zu verschiedenen Zeitpunkten, ob eine neue Programmversion vorliegt, und führt ggf. ein Update aus:
1. Beim Programmstart
2. Regelmäßig im laufenden Betrieb (derzeit alle 4 Stunden)
3. Wenn bei einer Anfrage von Benutzerdaten an den Server, die Programmversion nicht mehr mit der auf dem Server hinterlegten Version übereinstimmt.

Wenn im laufenden Betrieb ein Update ansteht, wird zunächst geprüft, ob derzeit ein Chip auf dem Lesegerät aufliegt. Ist dies der Fall, wird das Update zunächst zurückgestellt und erst ausgeführt, sobald der Chip vom Leser entfernt wird.

Wird ein Update durchgeführt, wird Ident-PLUS kurzzeitig beendet und sofort neu gestartet. Sollte zuvor die Konsole über das Traymenü geöffnet worden sein, wird sie beim Neustart nicht automatisch wieder geöffnet.

## Unterstützte Hardware


#### [TS-HRW38-USB](http://www.gis-net.de/rfid/deutsch/13_56mhz/ts_hrw38.htm)      
Reader der Firma GiS mbH.  
Diese Geräte werden per USB an den Rechner angeschlossen und kommunizieren  mit Ident-PLUS über einen virtuellen COM-Port. Beim Start des Programms wird die Windows-Registry nach einem PnP-Gerät mit passender Vendor-ID (`VID_1C40`) und Product-ID (`PID_05AC`) gesucht und der verwendete COM-Port ausgelesen. Anschließend verbindet sich Ident-PLUS mit diesem Gerät.  
Damit die Daten in der erwarteten Form ausgegeben werden, müssen die Reader vor der ersten Verwendung noch entsprechend programmiert werden. Dazu wird von der [Hersteller-Webseite](http://www.gis-net.de/rfid/deutsch/13_56mhz/ts_hrw38.htm) das [ReaderSetup-Tool](http://www.gis-net.de/rfid/software/GiS%20TS-HRW%20ReaderSetup.exe) heruntergeladen und unter 'Konfigurieren' mit dem Button 'Laden' die Konfigurationsdaten aus der Datei `Chipleser TS-HRW38.mkr` geladen. Anschließend werden diese durch den Button 'Programmieren' auf den Reader übertragen.
