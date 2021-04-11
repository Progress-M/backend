START TRANSACTION;

CREATE TABLE email_code (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    code text NULL,
    email text NULL,
    created_date_time timestamp without time zone NOT NULL,
    CONSTRAINT pk_email_code PRIMARY KEY (id)
);

CREATE TABLE files (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    bytes bytea NULL,
    CONSTRAINT pk_files PRIMARY KEY (id)
);

CREATE TABLE product_category (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    name text NULL,
    age_limit integer NOT NULL,
    image_id integer NULL,
    CONSTRAINT pk_product_category PRIMARY KEY (id),
    CONSTRAINT fk_product_category_files_image_id FOREIGN KEY (image_id) REFERENCES files (id) ON DELETE RESTRICT
);

CREATE TABLE "user" (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    name text NULL,
    is_man boolean NOT NULL,
    latitude double precision NOT NULL,
    longitude double precision NOT NULL,
    birth_year timestamp without time zone NOT NULL,
    player_id text NULL,
    image_id integer NULL,
    CONSTRAINT pk_user PRIMARY KEY (id),
    CONSTRAINT fk_user_files_image_id FOREIGN KEY (image_id) REFERENCES files (id) ON DELETE RESTRICT
);

CREATE TABLE company (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    latitude double precision NOT NULL,
    longitude double precision NOT NULL,
    time_zone text NULL DEFAULT 'Asia/Novosibirsk',
    name_official text NULL,
    name text NULL,
    representative text NULL,
    phone text NULL,
    email text NULL,
    inn text NULL,
    password text NULL,
    address text NULL,
    time_of_work text NULL,
    email_confirmed boolean NOT NULL,
    player_id text NULL,
    product_category_id integer NULL,
    image_id integer NULL,
    CONSTRAINT pk_company PRIMARY KEY (id),
    CONSTRAINT fk_company_files_image_id FOREIGN KEY (image_id) REFERENCES files (id) ON DELETE RESTRICT,
    CONSTRAINT fk_company_product_category_product_category_id FOREIGN KEY (product_category_id) REFERENCES product_category (id) ON DELETE RESTRICT
);

CREATE TABLE company_notification (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    create_time timestamp without time zone NOT NULL,
    company_id integer NULL,
    title text NULL,
    text text NULL,
    CONSTRAINT pk_company_notification PRIMARY KEY (id),
    CONSTRAINT fk_company_notification_company_company_id FOREIGN KEY (company_id) REFERENCES company (id) ON DELETE RESTRICT
);

CREATE TABLE favorite_company (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    user_id integer NOT NULL,
    company_id integer NOT NULL,
    CONSTRAINT pk_favorite_company PRIMARY KEY (id),
    CONSTRAINT fk_favorite_company_company_company_id FOREIGN KEY (company_id) REFERENCES company (id) ON DELETE CASCADE,
    CONSTRAINT fk_favorite_company_user_user_id FOREIGN KEY (user_id) REFERENCES "user" (id) ON DELETE CASCADE
);

CREATE TABLE message (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    sending_time timestamp without time zone NOT NULL,
    is_user_message boolean NOT NULL,
    user_id integer NULL,
    company_id integer NULL,
    text text NULL,
    CONSTRAINT pk_message PRIMARY KEY (id),
    CONSTRAINT fk_message_company_company_id FOREIGN KEY (company_id) REFERENCES company (id) ON DELETE RESTRICT,
    CONSTRAINT fk_message_user_user_id FOREIGN KEY (user_id) REFERENCES "user" (id) ON DELETE RESTRICT
);

CREATE TABLE offer (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    like_counter integer NOT NULL,
    text text NULL,
    create_date timestamp without time zone NOT NULL,
    sending_time timestamp without time zone NOT NULL,
    date_start timestamp without time zone NOT NULL,
    date_end timestamp without time zone NOT NULL,
    time_start timestamp without time zone NOT NULL,
    time_end timestamp without time zone NOT NULL,
    company_id integer NULL,
    image_id integer NULL,
    percentage integer NOT NULL,
    for_man boolean NOT NULL,
    for_woman boolean NOT NULL,
    upper_age_limit integer NOT NULL,
    lower_age_limit integer NOT NULL,
    CONSTRAINT pk_offer PRIMARY KEY (id),
    CONSTRAINT fk_offer_company_company_id FOREIGN KEY (company_id) REFERENCES company (id) ON DELETE RESTRICT,
    CONSTRAINT fk_offer_files_image_id FOREIGN KEY (image_id) REFERENCES files (id) ON DELETE RESTRICT
);

CREATE TABLE liked_offer (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    user_id integer NOT NULL,
    offer_id integer NOT NULL,
    CONSTRAINT pk_liked_offer PRIMARY KEY (id),
    CONSTRAINT fk_liked_offer_offer_offer_id FOREIGN KEY (offer_id) REFERENCES offer (id) ON DELETE CASCADE,
    CONSTRAINT fk_liked_offer_user_user_id FOREIGN KEY (user_id) REFERENCES "user" (id) ON DELETE CASCADE
);

CREATE TABLE stories (
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
    user_id integer NOT NULL,
    offer_id integer NOT NULL,
    CONSTRAINT pk_stories PRIMARY KEY (id),
    CONSTRAINT fk_stories_offer_offer_id FOREIGN KEY (offer_id) REFERENCES offer (id) ON DELETE CASCADE,
    CONSTRAINT fk_stories_user_user_id FOREIGN KEY (user_id) REFERENCES "user" (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_company_email ON company (email);

CREATE INDEX ix_company_image_id ON company (image_id);

CREATE UNIQUE INDEX ix_company_inn_address ON company (inn, address);

CREATE UNIQUE INDEX ix_company_player_id ON company (player_id);

CREATE INDEX ix_company_product_category_id ON company (product_category_id);

CREATE INDEX ix_company_notification_company_id ON company_notification (company_id);

CREATE INDEX ix_favorite_company_company_id ON favorite_company (company_id);

CREATE INDEX ix_favorite_company_user_id ON favorite_company (user_id);

CREATE INDEX ix_liked_offer_offer_id ON liked_offer (offer_id);

CREATE INDEX ix_liked_offer_user_id ON liked_offer (user_id);

CREATE INDEX ix_message_company_id ON message (company_id);

CREATE INDEX ix_message_user_id ON message (user_id);

CREATE INDEX ix_offer_company_id ON offer (company_id);

CREATE INDEX ix_offer_image_id ON offer (image_id);

CREATE INDEX ix_product_category_image_id ON product_category (image_id);

CREATE INDEX ix_stories_offer_id ON stories (offer_id);

CREATE INDEX ix_stories_user_id ON stories (user_id);

CREATE INDEX ix_user_image_id ON "user" (image_id);

COMMIT;


