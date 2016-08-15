begin transaction;

Create Table "Sizes" (
"idnum" int identity primary key,
"Sizes" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Sizes" (sizes) Values ('small'),
		('medium'),
		('large'),
		('x-large');

Create Table "Crust" (
"idnum" int identity primary key,
"Crust" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Crust" (crust) Values ('regular'),
		('thin');

Create Table "CrustOption" (
"idnum" int identity primary key,
"Flavoring" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "CrustOption" (flavoring) Values ('plain'),
			('pretzal'),
			('cheese-stuffed'),
			('garlic');

Create Table "Cheese" (
"idnum" int identity(0,1) primary key,
"Cheese" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Cheese" (cheese) values ('none'),
		('Mozzarella'),
		('Cheddar'),
		('Reduced Fat'),
		('Feta');

Create Table "Sauces" (
"idnum" int identity primary key,
"Sauces" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "sauces" (sauces) values ('Marinera Sauce'),
		('BBQ Sauce'),
		('Garlic Sauce'),
		('Alfredo Sauce');

Create Table "Meat" (
"idnum" int identity primary key,
"Meat" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "meat" ("meat") values ('Anchovies'),
		('Bacon'),
		('Chicken'),
		('Ham'),
		('Pepperoni'),
		('Salami'),
		('Sausage'),
		('Tofu');

Create Table "Misc" (
"idnum" int identity primary key,
"nonMeat" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "misc" (nonmeat) values ('Broccoli'),
		('Green Olives'),
		('Green Peppers'),
		('Mushrooms'),
		('Red Onions'),
		('Roasted Garlic'),
		('Pineapples'),
		('Yellow Banana Pepper');

Create Table "Qty" (
"idnum" int identity primary key,
"Quantity" nvarchar(15),
"Prices" decimal(3,2)
);

insert into "qty" ("Quantity") values ('Light'),
			('Regular'),
			('Extra'),
			('Double'),
			('Triple');

--Create Table "Spread" ( 
--Whole, Left, Right
--"idnum" int identity primary key,
--"Quantity" nvarchar(15),
--"Prices" decimal(3,2)
--);

create table "ItemType" (
"idnum" int identity primary key,
"type" nvarchar(10)
);

insert into "itemtype" (type) values ('pizza'),
('drink'),
('wings'), --spicey,buffalo,mild,bbq,crispy
('garlic bread'), --sides
('breadsticks');

create table "wingsoption"(
"idnum" int identity primary key,
"flavors" nvarchar (10)
--level of heat and spicey-ness
);

insert into "wingsoption" (flavors) values ('spicey');
insert into "wingsoption" (flavors) values ('buffalo');
insert into "wingsoption" (flavors) values ('mild');
insert into "wingsoption" (flavors) values ('bbq');
insert into "wingsoption" (flavors) values ('terriyaki');
insert into "wingsoption" (flavors) values ('honey');
insert into "wingsoption" (flavors) values ('crispy');

create table "sideoption"(
"idnum" int identity primary key,
"sides" nvarchar (10)
);

insert into "sideoption" (sides) values ('garlic br.');
insert into "sideoption" (sides) values ('breadstick');

create table "drinkoption" (
"idnum" int identity primary key,
"drinks" nvarchar (10)
)

insert into "drinkoption" (drinks) values ('brand1 sm');
insert into "drinkoption" (drinks) values ('brand1 lg');
insert into "drinkoption" (drinks) values ('brand2 sm');
insert into "drinkoption" (drinks) values ('brand2 lg');

create table "orderstatus" (
"idnum" int identity primary key,
"status" nvarchar(10)
);

insert into "orderstatus" (status) values ('not ready'),
('a little bit'),
('almost'),
('ready');

Create Table "InvoiceOrder" (
"idnum" int identity primary key,
"line" int,
"type" int,
"item" nvarchar(15), --itemvalue : product info (1,0,2.0,...)
"Prices" decimal(3,2),
"sessionid" nvarchar(30)
);

Create table "SaleTransaction" (
"transid" int identity primary key,
"name" nvarchar(25),
"total" money,
"store" nvarchar(),
"status" int default(1)
"orderdate" datetime default(getdate()),
"ordertype" bit  --True:Delivery|False:CarryOut
);

commit;


--------------------------
Create Table "Sizes" (
"idnum" int identity primary key,
"Sizes" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Sizes" (sizes) Values ('small');
Insert into "Sizes" (sizes) Values 		('medium');
Insert into "Sizes" (sizes) Values 		('large');
Insert into "Sizes" (sizes) Values 		('x-large');

Create Table "Crust" (
"idnum" int identity primary key,
"Crust" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Crust" (crust) Values ('regular');
Insert into "Crust" (crust) Values 		('thin');

Create Table "CrustOption" (
"idnum" int identity primary key,
"Flavoring" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "CrustOption" (flavoring) Values ('plain'),
Insert into "CrustOption" (flavoring) Values			('pretzal'),
Insert into "CrustOption" (flavoring) Values			('cheese-stuffed'),
Insert into "CrustOption" (flavoring) Values			('garlic');

Create Table "Cheese" (
"idnum" int identity(0,1) primary key,
"Cheese" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

Insert into "Cheese" (cheese) values ('none'),
Insert into "Cheese" (cheese) values		('Mozzarella'),
Insert into "Cheese" (cheese) values		('Cheddar'),
Insert into "Cheese" (cheese) values		('Reduced Fat'),
Insert into "Cheese" (cheese) values		('Feta');

Create Table "Sauces" (
"idnum" int identity primary key,
"Sauces" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "sauces" (sauces) values ('Marinera Sauce'),
insert into "sauces" (sauces) values 		('BBQ Sauce'),
insert into "sauces" (sauces) values 		('Garlic Sauce'),
insert into "sauces" (sauces) values 		('Alfredo Sauce');

Create Table "Meat" (
"idnum" int identity primary key,
"Meat" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "meat" ("meat") values ('Anchovies'),
insert into "meat" ("meat") values		('Bacon'),
insert into "meat" ("meat") values		('Chicken'),
insert into "meat" ("meat") values		('Ham'),
insert into "meat" ("meat") values		('Pepperoni'),
insert into "meat" ("meat") values		('Salami'),
insert into "meat" ("meat") values		('Sausage'),
insert into "meat" ("meat") values		('Tofu');

Create Table "Misc" (
"idnum" int identity primary key,
"nonMeat" nvarchar(15),
"Prices" decimal(3,2),
"Cost" decimal(3,2)
);

insert into "misc" (nonmeat) values ('Broccoli'),
insert into "misc" (nonmeat) values		('Green Olives'),
insert into "misc" (nonmeat) values		('Green Peppers'),
insert into "misc" (nonmeat) values		('Mushrooms'),
insert into "misc" (nonmeat) values		('Red Onions'),
insert into "misc" (nonmeat) values		('Roasted Garlic'),
insert into "misc" (nonmeat) values		('Pineapples'),
insert into "misc" (nonmeat) values		('Y. Banana Peppr');

Create Table "Qty" (
"idnum" int identity primary key,
"Quantity" nvarchar(15),
"Prices" decimal(3,2)
);

insert into "qty" ("Quantity") values ('Light'),
insert into "qty" ("Quantity") values			('Regular'),
insert into "qty" ("Quantity") values			('Extra'),
insert into "qty" ("Quantity") values			('Double'),
insert into "qty" ("Quantity") values			('Triple');

--Create Table "Spread" ( 
--Whole, Left, Right
--"idnum" int identity primary key,
--"Quantity" nvarchar(15),
--"Prices" decimal(3,2)
--);

create table "ItemType" (
"idnum" int identity primary key,
"type" nvarchar(10)
);

insert into "itemtype" (type) values ('pizza'),
insert into "itemtype" (type) values ('drink'),
insert into "itemtype" (type) values ('wings'),
insert into "itemtype" (type) values ('garlic br.'),
insert into "itemtype" (type) values ('breadstick');

create table "orderstatus" (
"idnum" int identity primary key,
"status" nvarchar(10)
);

insert into "orderstatus" (status) values ('not ready'),
insert into "orderstatus" (status) values ('a little'),
insert into "orderstatus" (status) values ('almost'),
insert into "orderstatus" (status) values ('ready');

Create Table "InvoiceOrder" (
"idnum" int identity primary key,
"line" int,
"type" int,
"item" nvarchar(15),
"Prices" decimal(3,2),
"sessionid" nvarchar(30)
);

Create table "SaleTransaction" (
"transid" int identity primary key,
"name" nvarchar(25),
"total" money,
"store" nvarchar(5),
"status" int default(1),
"datestamp" datetime default(getdate()),
"ordertype" bit  --True:Delivery|False:CarryOut
);

commit;