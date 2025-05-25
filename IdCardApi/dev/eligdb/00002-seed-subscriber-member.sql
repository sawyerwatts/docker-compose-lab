do
$$

    declare
        id0         text = 'asdf';
        declare id1 text = 'fdsa';

    begin

        insert into subscriber (id, name)
        values (id0, 'Jane Doe'),
               (id1, 'John Smith');

        insert into member (suffix, name, subscriber_ck)
        values (0, 'Jane Doe', (select ck from subscriber where id = id0)),
               (1, 'Dan Doe', (select ck from subscriber where id = id0)),
               (0, 'John Smith', (select ck from subscriber where id = id1)),
               (1, 'Sally Smith', (select ck from subscriber where id = id1));

    end
$$;