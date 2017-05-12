# WhichClientWhichRules

This is a simple C# ASP.NET Core application which lets you view which rules affect which clients. 

It works fine, but is meant for further customization.

A live demo is available at http://159.203.39.180
```
Demo whitelisted credentials:
Email: testuser@gmail.com
Password: hunter2

Demo non-whitelisted credentials:
Email: unauthorized@users.com
Password: password
```

# Rules Detection

The rules<->clients detection is done by parsing the rule's script.

In order for the client to be properly detected, make sure to have an array named **clients** containing the clients for which the rule applies.

Here's the whitelist rule for the demo application:
```javascript
function (user, context, callback) {
  var clients = ['WhatClientWhichRules'];
  if(clients.indexOf(context.clientName) === -1) {
      return callback(null, user, context);
  }
  
  var whitelist = [ 'testuser@gmail.com' ]; //authorized users
  var userHasAccess = whitelist.some(
    function (email) {
      return email === user.email;
  });

  if (!userHasAccess) {
    return callback(new UnauthorizedError('Access denied.'));
  }
  
  callback(null, user, context);
}
```

![How it looks](http://i.imgur.com/WR7BXVW.png)

# Installation (local test)

1. Install .NET Core: https://www.microsoft.com/net/core

2. Clone this repository.

3. Create a new client and authorize it to use the Auth0 Management API. The application requires at least the `read:clients`, `read:client_keys` and `read:rules` scopes. 
See https://auth0.com/docs/api/management/v2/tokens#1-create-and-authorize-a-client. **Add http://YOUR_HOST/signin-auth0 to the Allowed Callback URLs and http://YOUR_HOST to the Allowed Logout URLs in the Auth0 interface**

4. Edit and fill out the fields whithin the Auth0 object in WhichClientWhichRules/appsettings.json

5. Launch your favorite shell, cd to the directory containing appsettings.json and execute:
```
dotnet restore
dotnet run
```

# Installation (production)

1. Install the app by completing all the steps you would do for a local test (explained above).

2. Follow instructions for your platform at https://docs.microsoft.com/en-us/aspnet/core/publishing/
