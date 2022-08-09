# Authentication Demo

This repo simulates a server-side web app with an existing user database
using ASP.NET Core Identity and ***also*** allowing logging with an external identity
provider like [Duende IdentityServer](https://duendesoftware.com/products/identityserver).

It also supports deep-linking with authorization and uses conditional challenge logic.

## Just Run It

All of the setup is already included in the project, which should be
ready-to-run with no prep or modifications (assuming you have the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)
installed).  The best bet is to run this in a debugger with any of the
following IDEs:

* [Visual Studio (Mac or Windows)](https://visualstudio.microsoft.com/)
* [JetBrains Rider](https://www.jetbrains.com/rider/)
* [VS Code](https://code.visualstudio.com/) -- Add the [C# Language extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

## Key Features

* Local login with ASP.NET Identity
* Alternative remote/federated login with [demo version of Duende IdentityServer](https://demo.duendesoftware.com/)
* Home page is unauthenticated - allows anonymous access
* All other pages require authenticated users
* Login button on nav will take you to local login page
* Login page includes a button for Duende IdentityServer login
* Default `Challenge` will take you to Duende IdentityServer
* If a query string parameter of `src=local` is present the `Challenge`
is the local login page
* The `MyCompanies` page will show the "Companies" that you have access to as defined by claims in the user database
* The `CompanyDetails` page has a query string parameter (`cid`) for the CompanyId
you're trying to see and will validate your access against claims -- it will
redirect you to the `MyCompanies` page if you don't have access.

## Database Notes

This application sets up a user database using SQLite (see `AuthenticationDemo/Auth/UserContext.cs`).
The local users that it sets up are listed in that file with passwords:

* erik@acme.com -- Password123!
* ali@mars.com -- Password123!
* mega@local.com -- Password123!

The database is a local file stored in
the `Environment.SpecialFolder.LocalApplicationData`
folder. (`C:\Users\USERNAME\AppData\Local` on Windows
and `/Users/USERNAME/.local/share` on Mac) and the filename is defined
by the `UserDbSQLiteFilename` configuration value in `appsettings.json`.

## No Auto-Provisioned Users

Any user you are trying to log in with must be pre-configured
in the user database. The logic for that is in the `DevUserHelper.CreateDevUsers`
method (look for `bob@someplace.com` - he is a user coming from Duende IdentityServer
and the user id of `2` from there is what is stored as the `ExternalSubjectId` in
the local user database).

## Next Steps

A simpler way of handling the `Challenge` logic might be to create a two-step
login process. Step 1 would be to prompt the user for a username, then in the
`POST` of that page, you would determine if the authentication request needed
to be forwarded to the IdentityServer or stay local and prompt the user for a password.

You could base the condition on whether the user was found and had an `ExternalSubjectId`
defined.

Note that you would want to pass a `login_hint` parameter with the authentication
request to the IdentityServer which should prevent the user from entering their
username again -- and **might** even trigger forwarding the auth request to an identity
provider further upstream (like AzureAD or Google or something) if the user is configured
with a third party identity provider on the IdentityServer.

## Security Notes

* **When deep linking, be careful about the redirect URL.** It should be a local redirect
URL or at least one that you are consciously aware of. An open redirect would present
problems where the user might THINK they are on a good site that you created, but might
be on a malicious site.
