create table member
(
    ck            bigint generated always as identity primary key,
    suffix        smallint not null,
    name          text     not null,
    subscriber_ck bigint   not null references subscriber (ck)
)
