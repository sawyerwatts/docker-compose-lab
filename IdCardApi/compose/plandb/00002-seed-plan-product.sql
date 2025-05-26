declare
    @plan_id0 varchar(8) = 'M1234567',
    @plan_id1 varchar(8) = 'D1234567',
    @product_id0 varchar(4) = '1234',
    @product_id1 varchar(4) = '4321'

insert into [plan] (id, name)
values (@plan_id0, 'Medical Plan A'),
       (@plan_id1, 'Dental Plan A')

insert into product (id, plan_ck)
values (@product_id0, (select ck from [plan] where id = @plan_id0)),
       (@product_id1, (select ck from [plan] where id = @plan_id1))
