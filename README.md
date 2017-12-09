# ist.dad.ogp
DAD-OGP (DAD Online Gaming Platform) with Pacman

Project for DAD, IST - 1st Semester 2017-2018

# Authors
- Carlos Gonçalves
- Bruno Henriques
- Luís Silva

# Description
The DAD project aims at implementing a simplified (and therefore far from complete) fault-tolerant real-time distributed gaming platform.

# Implemented Features
- Distributed Game
- Peer-To-Peer Chat
- PCS
- PuppetMaster (supports commands: LocalState, Unfreeze, Freeze, Crash, StartServer, StartClient)

# Missing Features
- Fault Tolerance
- PuppetMaster commands GlobalStatus and InjectDelay

# Running
- Build the pacman.sln solution (using the Debug target) using MS Visual Studio
- Launch the PuppetMaster executable (PuppetMaster\bin\Debug\PuppetMaster.exe)
- Launch the PCS executable (PCS\bin\Debug\PCS.exe) on each machine
- Use the PuppetMaster console to start a server and client(s)
