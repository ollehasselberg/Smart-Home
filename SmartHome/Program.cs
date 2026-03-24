using System;
using System.Collections.Generic;
using System.Linq;

// ---------------- Observer ----------------
public interface IObserver
{
    void Update(string message);
}

public interface ISubject
{
    void Attach(IObserver observer);
    void Detach(IObserver observer);
    void Notify();
}

public class Lamp : ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    public string Name { get; }
    public bool IsOn { get; private set; }

    public Lamp(string name) => Name = name;

    public void TurnOn()
    {
        IsOn = true;
        Notify();
    }

    public void TurnOff()
    {
        IsOn = false;
        Notify();
    }

    public void Attach(IObserver observer) => observers.Add(observer);
    public void Detach(IObserver observer) => observers.Remove(observer);

    public void Notify()
    {
        foreach (var obs in observers)
            obs.Update($"[{Name}] Lamp is now {(IsOn ? "ON" : "OFF")}");
    }
}

public class Dashboard : IObserver
{
    public void Update(string message) => Console.WriteLine($"[Dashboard] {message}");
}

public class Logger : IObserver
{
    public void Update(string message) => SingletonLogger.Instance.Log(message);
}

// ---------------- Singleton ----------------
public class SingletonLogger
{
    private static SingletonLogger instance;
    private SingletonLogger() { }
    public static SingletonLogger Instance => instance ??= new SingletonLogger();
    public void Log(string message) => Console.WriteLine($"[LOG] {message}");
}

// ---------------- Command ----------------
public interface ICommand
{
    void Execute();
}

public class TurnOnLampCommand : ICommand
{
    private Lamp lamp;
    private IModeStrategy mode;
    public TurnOnLampCommand(Lamp lamp, IModeStrategy mode)
    {
        this.lamp = lamp;
        this.mode = mode;
    }
    public void Execute()
    {
        if (mode.CanTurnOnLamp(lamp))
            lamp.TurnOn();
        else
            Console.WriteLine($"[Command] Cannot turn on {lamp.Name} in current mode.");
    }
}

public class TurnOffLampCommand : ICommand
{
    private Lamp lamp;
    public TurnOffLampCommand(Lamp lamp) => this.lamp = lamp;
    public void Execute() => lamp.TurnOff();
}

public class RemoteInvoker
{
    private List<ICommand> commandQueue = new List<ICommand>();

    public void AddCommand(ICommand command) => commandQueue.Add(command);

    public void ExecuteAll()
    {
        foreach (var cmd in commandQueue) cmd.Execute();
        commandQueue.Clear();
    }

    public void ReplayLast(int n)
    {
        var lastCommands = commandQueue.Skip(Math.Max(0, commandQueue.Count - n)).ToList();
        foreach (var cmd in lastCommands) cmd.Execute();
    }
}

// ---------------- Strategy ----------------
public interface IModeStrategy
{
    bool CanTurnOnLamp(Lamp lamp);
}

public class NormalMode : IModeStrategy
{
    public bool CanTurnOnLamp(Lamp lamp) => true;
}

public class EcoMode : IModeStrategy
{
    public bool CanTurnOnLamp(Lamp lamp)
    {
        // Exempel: EcoMode tillåter inte att alla lampor tänds samtidigt
        if (lamp.Name == "Bedroom") return false;
        return true;
    }
}

public class PartyMode : IModeStrategy
{
    public bool CanTurnOnLamp(Lamp lamp) => true; // alla lampor tänds
}

// ---------------- Facade ----------------
public class SmartHomeFacade
{
    private IModeStrategy mode;
    private RemoteInvoker invoker = new RemoteInvoker();
    private List<Lamp> lamps = new List<Lamp>();
    private Dashboard dashboard = new Dashboard();
    private Logger logger = new Logger();

    public IModeStrategy CurrentMode => mode;

    public void SetMode(IModeStrategy mode)
    {
        this.mode = mode;
        Console.WriteLine($"[Facade] Mode set to {mode.GetType().Name}");
    }

    public void AddDevice(Lamp lamp)
    {
        lamps.Add(lamp);
        lamp.Attach(dashboard);
        lamp.Attach(logger);
    }

    public Lamp GetLampByName(string name) => lamps.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public void RunCommand(ICommand command) => invoker.AddCommand(command);
    public void ExecuteCommands() => invoker.ExecuteAll();

    public void MorningRoutine()
    {
        Console.WriteLine("[Facade] Running Morning Routine...");
        foreach (var lamp in lamps)
        {
            if (mode.CanTurnOnLamp(lamp))
                lamp.TurnOn();
        }
    }
}

// ---------------- Program ----------------
class Program
{
    static void Main(string[] args)
    {
        var home = new SmartHomeFacade();

        // Skapa lampor
        var lamp1 = new Lamp("Living Room");
        var lamp2 = new Lamp("Kitchen");
        var lamp3 = new Lamp("Bedroom");

        home.AddDevice(lamp1);
        home.AddDevice(lamp2);
        home.AddDevice(lamp3);

        // Startläge
        home.SetMode(new NormalMode());

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n--- Smart Home Menu ---");
            Console.WriteLine("1: Turn ON a lamp");
            Console.WriteLine("2: Turn OFF a lamp");
            Console.WriteLine("3: Change Mode");
            Console.WriteLine("4: Run Morning Routine");
            Console.WriteLine("5: Exit");
            Console.Write("Select an option: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    TurnOnLamp(home);
                    break;
                case "2":
                    TurnOffLamp(home);
                    break;
                case "3":
                    ChangeMode(home);
                    break;
                case "4":
                    home.MorningRoutine();
                    break;
                case "5":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    static void TurnOnLamp(SmartHomeFacade home)
    {
        Console.Write("Which lamp to turn ON (Living Room / Kitchen / Bedroom)? ");
        var lampName = Console.ReadLine();
        var lamp = home.GetLampByName(lampName);
        if (lamp != null)
        {
            var cmd = new TurnOnLampCommand(lamp, home.CurrentMode);
            home.RunCommand(cmd);
            home.ExecuteCommands();
        }
        else
            Console.WriteLine("Lamp not found.");
    }

    static void TurnOffLamp(SmartHomeFacade home)
    {
        Console.Write("Which lamp to turn OFF (Living Room / Kitchen / Bedroom)? ");
        var lampName = Console.ReadLine();
        var lamp = home.GetLampByName(lampName);
        if (lamp != null)
        {
            var cmd = new TurnOffLampCommand(lamp);
            home.RunCommand(cmd);
            home.ExecuteCommands();
        }
        else
            Console.WriteLine("Lamp not found.");
    }

    static void ChangeMode(SmartHomeFacade home)
    {
        Console.WriteLine("Select Mode: 1-Normal, 2-Eco, 3-Party");
        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1": home.SetMode(new NormalMode()); break;
            case "2": home.SetMode(new EcoMode()); break;
            case "3": home.SetMode(new PartyMode()); break;
            default: Console.WriteLine("Invalid mode."); break;
        }
    }
}