--
-- PostgreSQL database dump
--

-- Dumped from database version 14.2 (Ubuntu 14.2-1.pgdg20.04+1)
-- Dumped by pg_dump version 14.2 (Ubuntu 14.2-1.pgdg20.04+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: guilds; Type: DATABASE; Schema: -; Owner: -
--

CREATE DATABASE guilds WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE = 'en_US.UTF-8';


\connect guilds

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: configs; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.configs (
    id numeric(20,0) NOT NULL,
    prefix character varying DEFAULT 'y!'::character varying NOT NULL,
    minimal boolean DEFAULT true,
    guesstime integer DEFAULT 60,
    autodelete boolean DEFAULT false,
    inline boolean DEFAULT true,
    hangmantime integer DEFAULT 300,
    hangmanallowwords boolean DEFAULT true
);


--
-- Data for Name: configs; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.configs (id, prefix, minimal, guesstime, autodelete, inline, hangmantime, hangmanallowwords) FROM stdin;
11101000101001	:)	f	1	t	f	10000000	f
\.


--
-- Name: configs configs_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.configs
    ADD CONSTRAINT configs_pkey PRIMARY KEY (id);


--
-- PostgreSQL database dump complete
--

