# Remote Build Server

About the Project:
----------------------

For implementing a software on large scale systems, the software is divided into large number of partitions, with each one depicting a specific functionality. The successful addition of a partition into a software involves successful testing and working of the expected functionality of the partition. As more functionalities are added covering more requirements, additional partitions are added simultaneously. With the introduction of each partition, testing of the functionality of existing partition and newly added partition ensures that the newly added functionality can be added to the software with no impact on the existing code. As the number of partitions increases, the number of functionalities also increases, thereby it takes a lot of time in testing the earlier partitions and the newly added partition before it is added to the software. To reduce the time in performing repetitive testing activities of the existing functionalities, the proposed solution is to create a build server that automates this testing process, by running the repetitive test sequences of the existing functionalities automatically as well as testing the new functionalities. Examples include of creating a test automation tool using Selenium to perform regression testing on the existing functionality to ensure that new functionality does not impact the existing functionality.

Moreover, from a development perspective, a developer will have different environment. Whenever a new code is checked in development environment, it is expected that all the dependencies are in place. A build server helps in maintaining a smoother workflow by reducing the chances of code breaking in production. Build server can notify the client, development and QA team in case of build failure. The production deployment is being made simple since the developers or QA members need not have to manually remember the steps required to generate the new build.

Steps To Be Followed:
-------------------------

Step 1:
--------
From the GUI, list of all the test case files present in repository will be displayed in the repository list box.

Step 2:
-------
To add more test case files, we can click on ‘Browse’ button and then the ‘Upload’ button to add new files.

Step 3:
-------
Click on the Button "Select files for test request". All the files selected in repository list box will now be populated in second listbox. 

Step 4:
-------
Now hit button "Create Test Request". Test request created with the selected test cases will be shown in final list box along with old test requests. 

Step 5:
--------
Now enter the number of child process to be created in the text box and click on "Create Process" button. The specified number of child processes will be created.

Step 6:
--------
Click on the ‘Build’ button. All the test requests will be built and the result of each of the test requests will be displayed in the corresponding child process's console window.

Step 7:
--------
Click on ‘Kill’ button to kill all the child processes.
