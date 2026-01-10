Master Plan: Project Aegis-Link (C2 Tactical Simulator)
0. General Directives for Antigravity
Coding Style: Use Allman Style (braces on new lines).

Documentation: No comments in the code. Instead, write a detailed TECH_DEBT.md and ARCHITECTURE.md in a docs/ folder explaining every major decision.

Naming: * Private fields: _camelCase (e.g., _udpClient).

Public Properties/Methods: PascalCase.

Interfaces: Start with I (e.g., IDisposable).

Architecture: Strict Clean Architecture. Dependencies must flow inward: App -> Core.

1. Phase 1: The Core (The "Atoms")
The goal is to build the data structures and security logic without any UI or Network dependencies.

Task 1.1: Constants & Enums: Define a Constants class for the hardcoded PC IP, the Port (e.g., 5005), and the XOR Secret Key. Create an SystemStatus Enum: Idle, Connected, Armed, CommLoss.

Task 1.2: The Telemetry Frame: Create a readonly struct called TelemetryFrame. It must contain: Azimuth (float), Elevation (float), Battery (int), and Status (Enum).

Instruction: Use a struct because this is high-frequency data; we want it on the Stack, not the Heap, to avoid Garbage Collection (GC) pauses during the flight of a missile.

Task 1.3: XOR Cipher: Create a static class SecurityUtils. Implement a method ApplyXor(byte data, byte key).

Instruction: This is the "Secret Handshake." Explain in the ARCHITECTURE.md that we use this to validate the hardware is ours.

Task 1.4: Validation Logic: Create a class LaunchValidator. Implement a method that returns a bool based on safety rules (e.g., Battery > 10%, Status == Armed).

2. Phase 2: The Data Link (The "Nervous System")
The goal is to handle the "Radio" (UDP) in a separate thread so the UI never lags.

Task 2.1: The Communication Interface: Define an IDataLink interface with methods: Start(), Stop(), and an event DataReceived. Implement IDisposable to ensure the Socket is released.

Task 2.2: The UDP Service: Create UdpLinkService.

Instruction: Use UdpClient. Run the "Listening Loop" inside a Task.Run() to keep it asynchronous.

Instruction: When data arrives, use the SecurityUtils to "unlock" the packet before passing it to the ViewModel.

Task 2.3: The Watchdog (Heartbeat): Create a background timer that runs every 500ms. If the UdpLinkService hasn't received a packet since the last tick, it must trigger a "CommLoss" event.

3. Phase 3: The Dashboard (The "Truck")
The goal is to build a professional MVVM UI from scratch.

Task 3.1: MVVM Plumbing: * Create ViewModelBase which implements INotifyPropertyChanged.

Create RelayCommand which implements ICommand.

Instruction: No external libraries allowed. We are proving we know how .NET works.

Task 3.2: The MainViewModel: * Hold an ObservableCollection<string> for the "Command Log."

Properties for Azimuth, Elevation, and ConnectionStatus.

Instruction: When the Telemetry Struct arrives from the service, the ViewModel must update its properties. This will automatically update the UI via Data Binding.

Task 3.3: Aero-Space Modern UI: * Use a Dark Theme: Background #121212.

Accent Colors: Cyan (#00FFFF) for active, Amber (#FFBF00) for warnings.

Create a "Glassy" effect using Opacity and Blur on UI panels.

4. Phase 4: Hardware Integration (The "Launcher")
The goal is to make the Cardputer act as the field terminal.

Task 4.1: The ESP32 Loop: * Setup Wi-Fi in Station mode.

Send the TelemetryFrame every 100ms as a raw byte array.

Task 4.2: The Handshake: * Wait for a "Challenge" byte from the PC.

Apply the XOR math and send it back.

Only start sending telemetry once the PC "Accepts" the handshake.

Task 4.3: User Inputs: Map the Cardputer "F" key to send a "FIRE" byte to the PC. Map "A" to "ABORT".

5. Documentation Deliverables
README.md: How to run both apps.

ARCHITECTURE.md: Explain the use of Structs (Stack vs Heap), UDP (Low latency), and Asynchronous Tasks (UI responsiveness).

SECURITY.md: Explain the XOR Handshake as a defense against unauthorized hardware spoofing.

6. Phase 5: Tactical Refinement (10 Missing Essentials)

1. The Global Constants RegistryInstruction: Do not allow magic numbers (like 500ms or 0xAF) scattered in the code. Create a AegisLink.Core.Configuration namespace.Implementation: Use a public static class TacticalConstants to house network ports, timeout thresholds, and bitwise keys.2. Thread-Safe Observable LoggingInstruction: The Command Log cannot be a simple list. It must handle high-speed updates from the background network thread without crashing the UI.Implementation: Use BindingOperations.EnableCollectionSynchronization on your ObservableCollection. This is a "Senior-only" trick to allow background threads to add logs directly to the UI-bound list.3. Defensive Packet Validation (The "Sanity Check")Instruction: Before updating the UI, the ViewModel must validate the telemetry.Implementation: If Azimuth is $> 360$ or Elevation is $< 0$, mark the packet as CORRUPT in the log and ignore it. This proves you understand system safety.4. Custom XAML Control TemplatesInstruction: Don't use standard Windows buttons.Implementation: Redefine the ControlTemplate for the "FIRE" button. Give it a "Safety Cover" (a visual border) that only turns bright red when the SystemStatus is Armed.5. IDisposable Implementation on ServicesInstruction: Since we are using UDP Sockets, we must prevent memory leaks.Implementation: Ensure the UdpLinkService implements IDisposable. Antigravity must explicitly close the UdpClient and stop the background Task when the application closes.6. The "Silent" Watchdog HeartbeatInstruction: The UI shouldn't just "freeze" if the Cardputer dies.Implementation: Use a System.Timers.Timer in the ViewModel. If no valid packet has arrived in 1.5 seconds, the UI must automatically dim and show a "LINK LOST" watermark.7. Bitwise Bit-Packing (Advanced)Instruction: To simulate real-world low-bandwidth radio links, we won't send JSON or strings.Implementation: Pack the data into a byte[]. Azimuth as 2 bytes, Elevation as 2 bytes, and 1 byte for flags (Armed/Lock/Battery). This shows you know how to save "bandwidth" in a tactical environment.8. Value Converters for UI FeedbackInstruction: Don't write if(battery < 20) label.Color = Red in C#.Implementation: Create an IValueConverter in XAML called BatteryToColorConverter. This keeps the "View" logic inside the XAML where it belongs.9. Asynchronous Command HandlingInstruction: When the user clicks "Connect," the UI should show a "Connecting..." spinner.Implementation: Use an AsyncRelayCommand pattern. The button should be disabled (IsEnabled = false) while the Task is waiting for the Cardputer's XOR handshake.10. The "Black Box" Local LoggerInstruction: Every event must be saved for "After Action Review" (AAR).Implementation: Use System.IO.File.AppendAllLinesAsync to save every command sent and every telemetry frame received into a .txt file with a timestamp.7. Project Documentation Requirements (The "Senior" Paperwork)Antigravity must generate three specific files in the docs/ folder:C2_PROTOCOL.md: A table showing exactly which byte represents which data point (e.g., Byte 0-1: Azimuth).MEMORY_STRATEGY.md: A text file explaining why we chose Structs over Classes (Stack vs Heap) to avoid Garbage Collector (GC) latency.FAIL_SAFE.md: A document describing the 3 states of failure: Packet Loss, Handshake Failure, and Unauthorized Hardware.


8. Section 8: Mission Readiness & Engineering Lifecycle
To ensure the project is "Rafael-ready," Antigravity must adhere to these final operational constraints:

1. The "Single Source of Truth" (SSOT)
Instruction: Define a ProtocolVersion constant. If the Cardputer sends a packet from version 1.0 and the PC is on 2.0, the PC must reject the link and log a "Version Mismatch" error.

2. Dependency Injection (DI) Lite
Instruction: Do not "new up" the UdpLinkService inside the ViewModel.

Implementation: Pass the service through the ViewModel's constructor. This shows you understand Inversion of Control (IoC)—a key Phase 3 curriculum concept.

3. Graceful Degradation
Instruction: If the Wi-Fi card on the PC is disabled, the app should not crash.

Implementation: Wrap the Socket initialization in a try-catch that updates a StatusMessage string in the UI with a "Hardware Radio Unavailable" warning.

4. Git Flow & Atomic Commits
Instruction: Antigravity must act as if it's working in a team.

Implementation: It must describe its changes in "Sprints." For example: "Completed Phase 1: Core Structs and XOR Logic."

5. Deployment Simulation (The "Truck" Install)
Instruction: Create a Publish profile.

Implementation: The app should be able to run as a single .exe (Self-contained) so it can be "installed" on the ruggedized laptop without needing the user to install the .NET SDK manually.


