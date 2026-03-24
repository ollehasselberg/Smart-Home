--- Smart Home Control Center ---

Detta är en interaktiv app där man kan kontrollera belysning för ett smarthem-system. Programmet ger direkt feedback och logs för varje ändring som sker.

1. Hur man kör programmet
   Programmet är textbaserat och erbjuder som en smartkontroll-funktion. När man kör programmet får man upp en meny som frågar vad användaren vill göra. Användaren väljer ett alternativ (1-5) och får upp nya alternativ 
   baserat på tidigare val. Om man till exempel väljer 'Turn ON a light' så får man sedan välja vilken lampa man vill sätta på, och man får upp en logg som säger "x lampan är nu tänd".
   
2. Designmönster

2a. Observer 
Observer används för att notifiera olika delar av systemet när en enhet ändrar state.  
I projektet reagerar både 'Dashboard' och 'Logger' när en lampa tänds eller släcks.  
Det visar en “en-till-många”-relation där alla registrerade observers får live-uppdateringar.

2b. Command 
Command kapslar in åtgärder som objekt, vilket gör det möjligt att köa, exekvera och repetera dem.  
I koden används 'TurnOnLampCommand' och 'TurnOffLampCommand' för att tända och släcka lampor via 'RemoteInvoker'.  
Detta gör det enkelt att logga eller köa kommandon utan att behöva direkt åtkomst till lamporna.

2c. Strategy 
Strategy-mönstret används för att definiera olika lägen som styr lamporna. 
'NormalMode', 'EcoMode' och 'PartyMode' bestämmer t.ex. om en lampa får tändas eller inte, eller om flera lampor får vara tända samtidigt. 
Genom att byta strategi kan regler ändras utan att ändra övrig kod i systemet.

2d. Facade 
Facade ger ett förenklat gränssnitt mot hela systemet.  
'SmartHomeFacade' innehåller metoder som 'RunCommand', 'MorningRoutine', 'AddDevice' och 'SetMode'.  


2e. Singleton 
Singleton säkerställer att det finns en global instans som används överallt.  
I projektet används 'SingletonLogger' för loggning av alla händelser, vilket garanterar att samma logger används av både observers och kommandon.  

3. Demo: Öppna appen och 


