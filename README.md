# Teknik Web Services

Teknik is a suite of services with attractive and functional interfaces.

## Features
  * File Upload w/ encryption
  * Pastebin
  * URL Shortening
  * Blogs
  * Git Repositories
  * Podcasts
  * Easy to use API
  * Flexible installation and configuration

You can see a live demo [here](https://www.teknik.io).

## Requirements
In order to run Teknik on your server, you'll need:

  * IIS 7+ with URL Rewrite module or Apache with `mod_rewrite` enabled (Requires conversion of `web.config` files)
  * ASP.NET MVC 5
  * .NET Framework 4.6.2
  * MS SQL Server
  * hMailServer (If running email as well)
  * Web Mail Client (If you would like to have webmail)
  * Gogs Service (If you want to have Git integration)

## Installation
  * Clone the Teknik repository to your web root directory, or anywhere else you want to run Teknik from

```nohighlight
cd /var/www
git clone https://git.teknik.io/Teknikode/Teknik
```

  * Open the `Teknik.sln` file to build the project
  * Copy the build files to your `wwwroot` directory
  * Create a `ConnectionStrings.config` file in the `App_Data` directory and fill it with the following template and put in your SQL server connection details

  ```nohighlight
  <connectionStrings>
    <add name="TeknikEntities"
      providerName="System.Data.SqlClient"
      connectionString="Data Source=<server name>,<port>\<sql server name>;Initial Catalog=<database>;Integrated Security=False;User Id=<username>;Password=<password>" />
  </connectionStrings>
  ```

  * After the first run, a `Config.json` file will be created in the `App_Data` directory. This will need to be edited with your configuration options.

That's it, installation complete! If you're having problems, let us know through the [Contact](https://contact.teknik.io/) page.

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
