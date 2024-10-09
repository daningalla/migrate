-- migration: 240423a5-6ea8-4fc0-9ccf-145501847629
-- up:
alter table business.addresses add constraint address_account
    foreign key(accountId) references business.accounts(id);

-- down:
alter table business.addresses drop constraint address_account;
