PGDMP                          {            store    14.5    14.5 .    *           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                      false            +           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                      false            ,           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                      false            -           1262    32865    store    DATABASE     b   CREATE DATABASE store WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'Russian_Russia.1251';
    DROP DATABASE store;
                postgres    false            ?            1259    32866    administrator    TABLE     ?   CREATE TABLE public.administrator (
    id_administrator integer NOT NULL,
    email text NOT NULL,
    password text NOT NULL
);
 !   DROP TABLE public.administrator;
       public         heap    postgres    false            ?            1259    32871 "   administrator_id_administrator_seq    SEQUENCE     ?   ALTER TABLE public.administrator ALTER COLUMN id_administrator ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.administrator_id_administrator_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    209            ?            1259    32872    category    TABLE     d   CREATE TABLE public.category (
    id_category integer NOT NULL,
    category_name text NOT NULL
);
    DROP TABLE public.category;
       public         heap    postgres    false            ?            1259    32877    category_id_category_seq    SEQUENCE     ?   ALTER TABLE public.category ALTER COLUMN id_category ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.category_id_category_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    211            ?            1259    32878    customer    TABLE     ?   CREATE TABLE public.customer (
    id_customer integer NOT NULL,
    surname text NOT NULL,
    name text NOT NULL,
    patronymic text,
    email text NOT NULL,
    password text NOT NULL
);
    DROP TABLE public.customer;
       public         heap    postgres    false            ?            1259    32883    customer_id_customer_seq    SEQUENCE     ?   ALTER TABLE public.customer ALTER COLUMN id_customer ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.customer_id_customer_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    213            ?            1259    32884    main_log    TABLE     ?   CREATE TABLE public.main_log (
    id_log integer NOT NULL,
    id_customer integer NOT NULL,
    id_order integer NOT NULL,
    datetime timestamp without time zone NOT NULL
);
    DROP TABLE public.main_log;
       public         heap    postgres    false            ?            1259    32887    main_log_id_log_seq    SEQUENCE     ?   ALTER TABLE public.main_log ALTER COLUMN id_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.main_log_id_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    215            ?            1259    32888    product    TABLE     ?   CREATE TABLE public.product (
    id_product integer NOT NULL,
    title text NOT NULL,
    price money NOT NULL,
    id_category integer NOT NULL,
    images text[] NOT NULL,
    description text NOT NULL
);
    DROP TABLE public.product;
       public         heap    postgres    false            ?            1259    32893    product_id_product_seq    SEQUENCE     ?   ALTER TABLE public.product ALTER COLUMN id_product ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.product_id_product_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    217            ?            1259    32894    product_log    TABLE     ?   CREATE TABLE public.product_log (
    id_product_log integer NOT NULL,
    id_product integer NOT NULL,
    id_log integer NOT NULL
);
    DROP TABLE public.product_log;
       public         heap    postgres    false            ?            1259    32897    product_log_id_product_log_seq    SEQUENCE     ?   ALTER TABLE public.product_log ALTER COLUMN id_product_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.product_log_id_product_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    219            ?            1259    32898    store_order    TABLE     ?   CREATE TABLE public.store_order (
    id_order integer NOT NULL,
    id_product integer NOT NULL,
    id_customer integer NOT NULL,
    product_number integer NOT NULL
);
    DROP TABLE public.store_order;
       public         heap    postgres    false            ?            1259    32901    store_order_id_order_seq    SEQUENCE     ?   ALTER TABLE public.store_order ALTER COLUMN id_order ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.store_order_id_order_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);
            public          postgres    false    221                      0    32866    administrator 
   TABLE DATA           J   COPY public.administrator (id_administrator, email, password) FROM stdin;
    public          postgres    false    209   b5                 0    32872    category 
   TABLE DATA           >   COPY public.category (id_category, category_name) FROM stdin;
    public          postgres    false    211   ?5                 0    32878    customer 
   TABLE DATA           [   COPY public.customer (id_customer, surname, name, patronymic, email, password) FROM stdin;
    public          postgres    false    213   ?5                  0    32884    main_log 
   TABLE DATA           K   COPY public.main_log (id_log, id_customer, id_order, datetime) FROM stdin;
    public          postgres    false    215   A6       "          0    32888    product 
   TABLE DATA           ]   COPY public.product (id_product, title, price, id_category, images, description) FROM stdin;
    public          postgres    false    217   ^6       $          0    32894    product_log 
   TABLE DATA           I   COPY public.product_log (id_product_log, id_product, id_log) FROM stdin;
    public          postgres    false    219   O9       &          0    32898    store_order 
   TABLE DATA           X   COPY public.store_order (id_order, id_product, id_customer, product_number) FROM stdin;
    public          postgres    false    221   l9       .           0    0 "   administrator_id_administrator_seq    SEQUENCE SET     P   SELECT pg_catalog.setval('public.administrator_id_administrator_seq', 1, true);
          public          postgres    false    210            /           0    0    category_id_category_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public.category_id_category_seq', 2, true);
          public          postgres    false    212            0           0    0    customer_id_customer_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public.customer_id_customer_seq', 2, true);
          public          postgres    false    214            1           0    0    main_log_id_log_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public.main_log_id_log_seq', 1, false);
          public          postgres    false    216            2           0    0    product_id_product_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('public.product_id_product_seq', 8, true);
          public          postgres    false    218            3           0    0    product_log_id_product_log_seq    SEQUENCE SET     M   SELECT pg_catalog.setval('public.product_log_id_product_log_seq', 1, false);
          public          postgres    false    220            4           0    0    store_order_id_order_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('public.store_order_id_order_seq', 10, true);
          public          postgres    false    222            {           2606    32903     administrator Administrator_pkey 
   CONSTRAINT     n   ALTER TABLE ONLY public.administrator
    ADD CONSTRAINT "Administrator_pkey" PRIMARY KEY (id_administrator);
 L   ALTER TABLE ONLY public.administrator DROP CONSTRAINT "Administrator_pkey";
       public            postgres    false    209            }           2606    32905    category Category_pkey 
   CONSTRAINT     _   ALTER TABLE ONLY public.category
    ADD CONSTRAINT "Category_pkey" PRIMARY KEY (id_category);
 B   ALTER TABLE ONLY public.category DROP CONSTRAINT "Category_pkey";
       public            postgres    false    211                       2606    32907    customer customer_pkey 
   CONSTRAINT     ]   ALTER TABLE ONLY public.customer
    ADD CONSTRAINT customer_pkey PRIMARY KEY (id_customer);
 @   ALTER TABLE ONLY public.customer DROP CONSTRAINT customer_pkey;
       public            postgres    false    213            ?           2606    32909    main_log log_pkey 
   CONSTRAINT     S   ALTER TABLE ONLY public.main_log
    ADD CONSTRAINT log_pkey PRIMARY KEY (id_log);
 ;   ALTER TABLE ONLY public.main_log DROP CONSTRAINT log_pkey;
       public            postgres    false    215            ?           2606    32911    product_log product_log_pkey 
   CONSTRAINT     f   ALTER TABLE ONLY public.product_log
    ADD CONSTRAINT product_log_pkey PRIMARY KEY (id_product_log);
 F   ALTER TABLE ONLY public.product_log DROP CONSTRAINT product_log_pkey;
       public            postgres    false    219            ?           2606    32913    product product_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_pkey PRIMARY KEY (id_product);
 >   ALTER TABLE ONLY public.product DROP CONSTRAINT product_pkey;
       public            postgres    false    217            ?           2606    32915    store_order store_pkey 
   CONSTRAINT     Z   ALTER TABLE ONLY public.store_order
    ADD CONSTRAINT store_pkey PRIMARY KEY (id_order);
 @   ALTER TABLE ONLY public.store_order DROP CONSTRAINT store_pkey;
       public            postgres    false    221            ?           2606    32916    product fk_category    FK CONSTRAINT     ?   ALTER TABLE ONLY public.product
    ADD CONSTRAINT fk_category FOREIGN KEY (id_category) REFERENCES public.category(id_category);
 =   ALTER TABLE ONLY public.product DROP CONSTRAINT fk_category;
       public          postgres    false    3197    217    211            ?           2606    32921    store_order fk_customer    FK CONSTRAINT     ?   ALTER TABLE ONLY public.store_order
    ADD CONSTRAINT fk_customer FOREIGN KEY (id_customer) REFERENCES public.customer(id_customer);
 A   ALTER TABLE ONLY public.store_order DROP CONSTRAINT fk_customer;
       public          postgres    false    221    3199    213            ?           2606    32926    main_log fk_customer    FK CONSTRAINT     ?   ALTER TABLE ONLY public.main_log
    ADD CONSTRAINT fk_customer FOREIGN KEY (id_customer) REFERENCES public.customer(id_customer);
 >   ALTER TABLE ONLY public.main_log DROP CONSTRAINT fk_customer;
       public          postgres    false    215    3199    213            ?           2606    32931    product_log fk_log    FK CONSTRAINT     w   ALTER TABLE ONLY public.product_log
    ADD CONSTRAINT fk_log FOREIGN KEY (id_log) REFERENCES public.main_log(id_log);
 <   ALTER TABLE ONLY public.product_log DROP CONSTRAINT fk_log;
       public          postgres    false    3201    215    219            ?           2606    32936    main_log fk_order    FK CONSTRAINT     }   ALTER TABLE ONLY public.main_log
    ADD CONSTRAINT fk_order FOREIGN KEY (id_order) REFERENCES public.store_order(id_order);
 ;   ALTER TABLE ONLY public.main_log DROP CONSTRAINT fk_order;
       public          postgres    false    221    215    3207            ?           2606    32941    store_order fk_product    FK CONSTRAINT     ?   ALTER TABLE ONLY public.store_order
    ADD CONSTRAINT fk_product FOREIGN KEY (id_product) REFERENCES public.product(id_product);
 @   ALTER TABLE ONLY public.store_order DROP CONSTRAINT fk_product;
       public          postgres    false    217    221    3203            ?           2606    32946    product_log fk_product    FK CONSTRAINT     ?   ALTER TABLE ONLY public.product_log
    ADD CONSTRAINT fk_product FOREIGN KEY (id_product) REFERENCES public.product(id_product);
 @   ALTER TABLE ONLY public.product_log DROP CONSTRAINT fk_product;
       public          postgres    false    217    219    3203                  x?3?LL???s ?zE?.W? ?`f            x?3?tO?M-?2?V?)-?????? G??         t   x?3?t*?IL-?t??,)???t?K)JM-?L???Lt?M???+*?,,O-*?442?2??0????.?^?qa/??y@???9/LA??t??????\δ???????ܬ??$?=... ?'-?             x?????? ? ?      "   ?  x??T?n?0?N??7???????u???(?`tB??IM?8??uڻ?,<N:????|v?s???s?s*??&??߾&	z?x?xΗ?1?~	!e?7⍠%??QYAY\A?1?q?	??q??<'aL???i?Y~YX?,%?!????eBf)?<?Н"4,rV?4?8?Q$(??Gc?(H?Gy?K??-??0v {a6@?5(Z?????J????)???I?F{?g???*a7j?T-aH??|1?0Φ??????|p0e{?`+?`Nv;????sմ?`E?.??<??$?O?Oz<΄=NzDٳ??????`??i??q???c?ǳg_%G??G??"?'??̲.???ִ?? L???f@?\펝e
??V??ǫ??	????|?5"?;NodEEJ)?????;?? #=?u??QS[????u8Qn??*R??V	]I?6R?_???bv??#a??)q˟?='m?jxA??9Qtci!??A?pw?=?'A0??Q!?
?22?ԈKn[,??ڇ?u?砱??+	D̆*????g`?	6?Uc?????n??J)?o5WTֆצ?1?M?ogar19???k??Ƿwg͔/?t??<(??u?a?^??^?Û??-2ⰾ'"M??dqq?qu?~MȊޯ.??w???3??\?3e{AQ?????k7rΤ?v?7]V3??F?k??? ?^?GAh?~?R??1^g?\????M??F??~????      $      x?????? ? ?      &      x?????? ? ?     