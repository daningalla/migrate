-- migration: 24662cfa-15f4-4bba-acf2-c557652a9135
-- up:
create table business.accounts(
    id uuid not null primary key,
    firstName text not null,
    lastName text not null,
    picture bytea null,
    emailAddress text not null
);

create table business.addresses(
    id uuid not null primary key,
    accountId uuid not null,
    street1 text not null,
    street2 text null,
    city text not null,
    state text not null,
    postalCode text not null
);

-- down:
drop table business.addresses;
drop table business.accounts;
