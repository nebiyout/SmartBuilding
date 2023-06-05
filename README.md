# SmartBuilding

The project aims to simulate the operation of equipment within a building, 
offering a highly flexible architecture capable of accommodating various 
smart building components. While my particular focus was on implementing 
elevator operations, the design is intended to be easily extended to 
incorporate the functionality of other equipment as well.


The project consists of two console applications. The first application is 
responsible for simulating the elevator, while the second application 
serves as a display screen to visualize the elevator's movements. 
To set up this functionality, a multi-startup project configuration 
is required, with the "ConsoleDisplayScreen" set to start 
before the "SmartBuilding" project.


#Solution Structure

#Project 01(ConsoleDisplayScreen)
 - Program.cs => Listens data to display
 
#Project 02(SmartBuilding)
- Contracts  => interfaces for building,elevator, floor, passenger and display
- Core       => entities and DTo
- Services   => operations and notification of an elevator
- Utils      => Pubsub to show elevators position, direction and No. of passengers to the display ("ConsoleDisplayScreen")
                Helper clases for building and building Processor 
- Program.cs (menu navigation and user interactions)


Upon executing the application with the configuration, the 
"ConsoleDisplayScreen" will display the message "Waiting for connection...," 
indicating that it is ready to establish a connection. Simultaneously, 
the "SmartBuilding" console application will present a console menu:


