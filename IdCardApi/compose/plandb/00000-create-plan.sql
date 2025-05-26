create table [plan]
(
    ck   int identity (1, 1) not null primary key,
    id   varchar(8)          not null unique,
    name varchar(max)        not null
)