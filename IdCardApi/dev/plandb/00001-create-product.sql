create table product
(
    ck      int identity (1, 1) not null primary key,
    id      varchar(max)        not null,
    plan_ck int                 not null,
    constraint fk_product_plan_ck foreign key (plan_ck)
        references [plan](ck)
)
