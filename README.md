# Teknik Web Services

[![Build Status](https://uncled1023.visualstudio.com/_apis/public/build/definitions/47815734-d274-4bfd-8945-d58f2261b421/4/badge)](https://uncled1023.visualstudio.com/Teknik/_build/index?definitionId=4)

Teknik is a suite of services with attractive and functional interfaces.

## Features
  * File Upload w/ client side encryption
  * Album Support
  * Pastebin
  * URL Shortening
  * Blogs
  * Git Integration (Gitea)
  * Podcasts
  * Easy to use API
  * Flexible installation and configuration
  * And much more...

You can see a live demo [here](https://www.teknik.io).

## Requirements
In order to run Teknik on your server, you'll need:

  * IIS 7+, Nginx, or Apache
  * [.NET Core 2.1 Runtime](https://www.microsoft.com/net/download/) (Or SDK if building the src)
  * A SQL Server (MS SQL Server, MySQL, SQLite)
  * [hMailServer](https://www.hmailserver.com/download) (If running email service)
  * [Gitea](https://github.com/go-gitea/gitea) (If you want to have Git integration)
  * Web Mail Client (If you would like to have webmail)

## Installation
 * Set up Asp.Net Core to work with your system of choice - [Instructions](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-2.1&tabs=aspnetcore2x) 
 * Download the latest release for your system - [Releases](https://git.teknik.io/Teknikode/Teknik/releases)
 * Copy the files to your local web root directory.
 * Create a `ConnectionStrings.config` file in the `App_Data` directory and fill it with the following template and put in your SQL server connection details.

  ```nohighlight
  <connectionStrings>
    <add name="TeknikEntities"
      providerName="System.Data.SqlClient"
      connectionString="<Sql Server Connection String>" />
  </connectionStrings>
  ```

  * After the first run, a `Config.json` file will be created in the `App_Data` directory. This will need to be edited with your configuration options.

That's it, installation complete! If you're having problems, let us know through the [Contact](https://contact.teknik.io/) page.

## Building

### Linux
 * Clone the Teknik repository to a directory of your choosing.

```nohighlight
cd ~
mkdir src
cd ./src
git clone https://git.teknik.io/Teknikode/Teknik
```

* Install Node.js (Includes npm).
* Run the npm build script `npm run build`.
  * This will install, move, bundle, and minify all the client side assets (JavaScript, CSS, Fonts, and Images).
  * To see exactly what happens or to modify the bui;d. look at the `gulpfile.js` file.
* Run dotnet publish from the root directory of the repo to build and package the app into the release publish directory (for example: ./Teknik/bin/Release/netcoreapp2.1/publish).

```
dotnet publish --configuration Release
```

* Copy the files in the publish directory to where you want to run the website from.

* Test the website:
  * In the directory conatining the published files, run `dotnet Teknik.dll`
  * In a browser, open http://localhost:5000/?sub=www

## Authors and contributors
  * [Chris Woodward](https://www.teknik.io) (Creator, Developer)
  * [dronedaddy](https://www.behance.net/dronedaddy) (Logo Designer)

## License
[BSD 3-Clause license](http://opensource.org/licenses/BSD-3-Clause)

## Development
You can view Teknik's [Development Branch](https://dev.teknik.io/) to see the current new features (It may not work, as it is a development branch).

## Contributing
If you are a developer, we need your help. Teknik is a young project and we have lots of stuff to do. Some developers are contributing with new features, others with bug fixes. Any help you can give would be greatly appreciated!

## Further information
If you want to know more about the features of Teknik, check the [Help](https://help.teknik.io/) page. Also, if you're having problems with Teknik, let us know through the [Contact](https://contact.teknik.io/) page. Don't forget to give feedback and suggest new features! :)
