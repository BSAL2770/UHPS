# UHPS: https://uhps.azurewebsites.net/
Courier/Post office database: Gather data concerning the packages going in and out of the post office, where they are going, and who is sending them. Additionally, the team would have to build a database of customers in  relation to what packages they are shipping out.The database should have the functionality to provide ‘tracking’ history for packages both for customers and employees. 
This is a Database Systems course project to be graded by Professor Ramamurthy at University of Houston. Team members include Son La, Morrison Fowlks, Andy Nguyen, Josh Opie, Bader Salem. 

Contributors (contribution details in Todo and Progress log):
La, Son 
Opie, Josh
Fowlks, Morrison
Nguyen, Andy
Salem, Bader

This is a web application project to be submitted to Professor Uma Ramamurthy. This web application allows customers to track shipments sent at a store, estimate shipping cost, browse products, view their sent/received shipments,
and employees to manage (create, edit, view, delete) shop items, shipments, sales, stores, customers,... Anonymous users can track their shipments with a valid tracking number 

Basic dir/file descriptions:
1. Migrations: translate C# model classes into SQL server database language to create the database
2. DbInitilizer.cs: insert initial data for the database
3. ApplicationDBContext: specify db and tables structure
4. /wwwroot: contains css + icon files
5. /Models/: specify db relations, entities, and attributes
6. /Pages/: contain various pages for the web app
7. appsettings: contain app secret like sql server connectionstring
8. Programs.cs: where the application starts, specify services/modules being used
9. backuptrigger.sql: sql trigger code

Installation/Localhost:
1. Install Visual Studio Community and Microsoft SQL server Express (and set up local database)
2. Clone/Download the repository/Google Drive
3. Open UHPS\UHPostalService\UHPostalService.sln
4. Open Package Manager Console: Tools > Nuget Package Manager > Package Manager Console
5. run "update-database"
6. Run: Debug > Start Debugging

Login credentials (for testing):
1. Admin: admin@uhps.com - password
2. Supervisor: josh@uhps.com - josh
3. Employee: dan@uhps.com - dan, son@uhps.com - son
4. Customer: morrison@email.com - morrison

Notes about roles:
1. Admin - can see everything
2. Supervisor - can only see employees who have not yet been assigned their store, for the purpose of assigning them one, aswell as their store's employees
3. Employee - can only see the other employees working at their store, cannot edit or delete employees other than themselves
4. Customer - Can only see packages sent to or from them, cannot edit, delete, or create most things
5. Anonymous - can only see store locations, get a quote, and check the shop (our shop is in person, so you cannot buy things online), can see tracking record of a     package with the appropriate tracking number

Credits:
.NET Razor Pages Crash Course:
https://www.youtube.com/watch?v=eru2emiqow0
Database Models
https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?view=aspnetcore-6.0&tabs=visual-studio#scaffold-student-pages
Triggers:
https://www.sqlshack.com/learn-sql-sql-triggers/
Authentication:
https://www.tektutorialshub.com/asp-net-core/user-registration-login-using-cookie-authentication-asp-net-core/
Using raw sql in .NET Razor Pages:
https://docs.microsoft.com/en-us/ef/core/querying/raw-sql

To improve:
1. A new employee (store unassigned) should not have access to pages that an official employee can (e.g. create shop item)
2. An employee should not be able to edit their own role to supervisor/admin nor edit their workplace (storeid)
3. An employee should only see shipments that are in their store at the moment and shipments expected to arrive
4. Should have another relation storing sale items for each sale
5. Dropdown for multiple filters at once
6. Checkin multiple shipments in 1 form
7. Shopping Cart: https://learningprogramming.net/net/asp-net-core-razor-pages/build-shopping-cart-with-session-in-asp-net-core-razor-pages/ 
8. Stores Index: move phone number, remove zipcode/state from street address
9. Fix store index sort
10. shipments: remove newpackage/checkin buttons visibility for customers
11. default sort shipment index by date sent
12. shipment details: change link in store#; error clicking sender/receiver name link
13. anonymous should not edit employee (nor details, delete)
14. anonymous should not inxdex/edit/detail/delete customers
15. show total shipping cost for employees
16. create shipment/sale: if already customer just enter email
