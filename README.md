mToolkit Framework
==================

mToolkit Framework is an open-source, modular software development solution designed to facilitate the creation, deployment, and management of portable and self-contained tools and applications. It aims to empower developers to quickly develop custom tools for their personal use-cases and to share those tools with the community via open-source git repositories. With the mToolkit Framework, you can share tools like documents, turning software into shareable files that can be easily installed and utilized by others.

Features
--------

-   **Component Library**: A collection of classes and systems essential for tool operation, handling workspaces, configuration files, and inter-tool communication through the Pipeline infrastructure.
-   **Desktop Launcher**: A platform for loading and displaying tools' user interfaces, as well as managing tool installations from zip files.
-   **Tools**: Portable, self-contained C# and WPF-powered library classes that utilize a mTool class to create a UserControl for presenting the UI to users.

Getting Started
---------------

1.  Clone this repository to your local machine.
2.  Open the solution in Visual Studio 2022.
3.  Build the solution and run the Desktop Launcher application.

Usage
-----

1.  Develop your custom Tool using the mToolkit Framework's Component Library and the provided Example Tool as an WPF Class Library.
2.  Publish your Tool through Visual Studio using the 'Portable' Target Runtime.
3.  Share your Tool by uploading it to a git repository or directly sending the zip file to a friend.
4.  Install and use the Tool via the Desktop Launcher by either downloading it from the git repository or opening the received zip file.

Contributing
------------

We welcome contributions to the mToolkit Framework! Whether you're interested in developing new tools, enhancing existing ones, or improving the framework itself, your help is appreciated. Please follow these steps to contribute:

1.  Fork this repository.
2.  Create a new branch for your changes.
3.  Make your changes and commit them to your branch.
4.  Submit a pull request with a description of your changes.

License
-------

mToolkit Framework is released under the MIT License.
