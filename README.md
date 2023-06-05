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


Upon executing the application with the with the configuration, the 
"ConsoleDisplayScreen" will display the message "Waiting for connection...," 
indicating that it is ready to establish a connection. Simultaneously, 
the "SmartBuilding" console application will present the following prompt 
to the user:


Press #1 to create a building
Press #2 to exit

Please select an option: 1

Enter the Building Name:     <ENTER THE BUILDING NAME> and <Press Any Key>
Enter total No. of basement: <ENTER BASEMENT COUNT, GREATER OR EQUAL TO ZERO> and <Press Any Key>
Enter total No. of floor:    <ENTER FLOOR COUNT, GREATER THAN ZERO> and <Press Any Key>


Enter the total No. of elevator: <ENTER NUMBER OF ELEVATORS> and <Press Any Key>
-----------------------------
Enter the elevator #1 data
-----------------------------
Enter initial elevator floor No. between (min floor and max floor:) <ENTER FLOOR NUMBER> and <Press Any Key>
Enter elevator maximum head count : <ENTER MAXIMUM PERSON LIMIT> and <Press Any Key>


Press #1 to call elevator
Press #2 to move passangers, Pending(0)
Press #3 to view elevators status
Press #4 to view passengers status
Press #5 clear passenger
Press #6 to exit


#Select an option: 1
------------------------------
Press #1 to call elevator
------------------------------

Enter the total No. of passengers: <ENTER NUMBER OF PASSANGERS>
------------------------------
Enter the Passanger #1 data
------------------------------
Enter the passanger floor No. between (0 and 9): <ENTER FLOOR NO>
Enter direction Press (Up or Down(U/D)) :<ENTER DIRECTION U/D>


#This will move the elevator to the destination floor
=====Loading Passenger===
Enter destination floor between(0 and 9) for the passenger at floor No. #0:  <ENTER FLOOR NO> 





#Select an option: 2
#Press #2 to move passangers, Pending(0)
-----------------------------
Enter destination floor between(0 and 9) for the passenger at floor No. #0:  <ENTER FLOOR NO> 



#Select an option: 3
#Press #3 to view elevators status
-----------------------------
Elevator       : EL-1
Floor No.      : 0
Direction      : Idle
Max Head Count : 7
Status         : Available
Passengers     : 0
Callers        : 0
-----------------------------


#Select an option: 4
#Press #4 to view passengers status
-----------------------------
Elevator       : EL-1
-----------------------------
Passenger #       : 1
Source Floor      : 0
Destination Floor : -
Direction         : Up
Waiting           : False
-----------------#-----------


#Select an option: 5
#Press #5 to clear passenger
-----------------------------
Clear passengers


#Select an option: 
#Press #6 to exit
-----------------------------
back to main menu


