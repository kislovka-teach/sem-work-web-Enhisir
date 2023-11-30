--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1 (Debian 16.1-1.pgdg120+1)
-- Dumped by pg_dump version 16.1 (Debian 16.1-1.pgdg120+1)

-- Started on 2023-12-01 02:17:03 MSK

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
-- TOC entry 237 (class 1255 OID 17223)
-- Name: add_announcement(uuid, character varying, text, character varying, numeric, uuid, character varying); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.add_announcement(IN _id uuid, IN _title character varying, IN _description text, IN _address character varying, IN _price numeric, IN _city_id uuid, IN _owner_id character varying, OUT _result integer)
    LANGUAGE plpgsql
    AS $$
begin
	insert into "Announcement"(
		id,
		title,
		description,
		address,
		price,
		city_id,
		owner_id
	) values (
		_id,
		_title,
		_description,
		_address,
		_price,
		_city_id,
		_owner_id
	) returning 1 into _result;
	if exists (SELECT 1 FROM "City" WHERE "id" = _city_id)
	   and exists (SELECT 1 FROM "User" WHERE login = _owner_id) then
		commit;
	else
		rollback;
		_result=-1;
	end if;
end;
$$;


ALTER PROCEDURE public.add_announcement(IN _id uuid, IN _title character varying, IN _description text, IN _address character varying, IN _price numeric, IN _city_id uuid, IN _owner_id character varying, OUT _result integer) OWNER TO postgres;

--
-- TOC entry 235 (class 1255 OID 17222)
-- Name: add_message(uuid, uuid, character varying, text); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.add_message(IN _id uuid, IN _chat_id uuid, IN _sender_id character varying, IN _content text, OUT _result integer)
    LANGUAGE plpgsql
    AS $$	
Declare
	members members_dto;
begin
	select * from get_chat_participants(_chat_id) into members;
	
	insert into "Message"(
		id,
		chat_id,
		sender_id,
		content
	) values (
		_id,
		_chat_id,
		_sender_id,
		_content
	) returning 1 into _result;
	if exists (SELECT 1 FROM "Chat" WHERE "id" = _chat_id)
	   and (_sender_id=members.owner_id OR _sender_id=members.consumer_id) then
		commit;
	else
		rollback;
		_result=-1;
	end if;
end;
$$;


ALTER PROCEDURE public.add_message(IN _id uuid, IN _chat_id uuid, IN _sender_id character varying, IN _content text, OUT _result integer) OWNER TO postgres;

--
-- TOC entry 236 (class 1255 OID 17177)
-- Name: add_user(character varying, character varying, character varying, uuid); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.add_user(IN _name character varying, IN _login character varying, IN _password character varying, IN _city_id uuid, OUT _result integer)
    LANGUAGE plpgsql
    AS $$
begin
	insert into "User"(
		"name",
		login,
		"password",
		city_id
	) values (
		_name,
		_login,
		_password,
		_city_id
	) returning 1 into _result;
	if exists (SELECT 1 FROM "City" WHERE id = _city_id) then
		commit;
	else
		rollback;
		_result=-1;
	end if;
end;
$$;


ALTER PROCEDURE public.add_user(IN _name character varying, IN _login character varying, IN _password character varying, IN _city_id uuid, OUT _result integer) OWNER TO postgres;

--
-- TOC entry 234 (class 1255 OID 17194)
-- Name: get_chat_participants(uuid); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.get_chat_participants(_chat_id uuid) RETURNS TABLE(owner_id character varying, consumer_id character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
	RETURN QUERY
	select ant.owner_id, cht.consumer_id from "Chat" as cht 
				 join "Announcement" ant on cht.announcement_id=ant.id
	 			 where cht.id=_chat_id;
	RETURN;
END
$$;


ALTER FUNCTION public.get_chat_participants(_chat_id uuid) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 218 (class 1259 OID 16942)
-- Name: Announcement; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Announcement" (
    id uuid NOT NULL,
    title character varying(50) NOT NULL,
    description text NOT NULL,
    price numeric NOT NULL,
    city_id uuid NOT NULL,
    owner_id character varying NOT NULL,
    address character varying NOT NULL,
    privileged boolean DEFAULT false NOT NULL,
    disabled boolean DEFAULT false NOT NULL
);


ALTER TABLE public."Announcement" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16966)
-- Name: Chat; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Chat" (
    id uuid NOT NULL,
    announcement_id uuid NOT NULL,
    consumer_id character varying
);


ALTER TABLE public."Chat" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16998)
-- Name: City; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."City" (
    id uuid NOT NULL,
    name character varying NOT NULL
);


ALTER TABLE public."City" OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 16971)
-- Name: Message; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Message" (
    id uuid NOT NULL,
    chat_id uuid NOT NULL,
    content text NOT NULL,
    sender_id character varying NOT NULL,
    creation_time timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public."Message" OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 16937)
-- Name: User; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."User" (
    login character varying(20) NOT NULL,
    password character varying(400) NOT NULL,
    city_id uuid NOT NULL,
    name character varying
);


ALTER TABLE public."User" OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 17214)
-- Name: members_dto; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.members_dto (
    owner_id character varying,
    consumer_id character varying
);


ALTER TABLE public.members_dto OWNER TO postgres;

--
-- TOC entry 3391 (class 0 OID 16942)
-- Dependencies: 218
-- Data for Name: Announcement; Type: TABLE DATA; Schema: public; Owner: postgres
--


--
-- TOC entry 3234 (class 2606 OID 16948)
-- Name: Announcement Announcement_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Announcement"
    ADD CONSTRAINT "Announcement_pkey" PRIMARY KEY (id);


--
-- TOC entry 3236 (class 2606 OID 16970)
-- Name: Chat Chat_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Chat"
    ADD CONSTRAINT "Chat_pkey" PRIMARY KEY (id);


--
-- TOC entry 3240 (class 2606 OID 17002)
-- Name: City City_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."City"
    ADD CONSTRAINT "City_pkey" PRIMARY KEY (id);


--
-- TOC entry 3238 (class 2606 OID 16977)
-- Name: Message Message_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Message"
    ADD CONSTRAINT "Message_pkey" PRIMARY KEY (id);


--
-- TOC entry 3232 (class 2606 OID 17166)
-- Name: User User_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "User_pkey" PRIMARY KEY (login);


--
-- TOC entry 3242 (class 2606 OID 17167)
-- Name: Announcement Announcement_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Announcement"
    ADD CONSTRAINT "Announcement_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES public."User"(login) ON DELETE CASCADE NOT VALID;


--
-- TOC entry 3245 (class 2606 OID 17172)
-- Name: Message Message_owner_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Message"
    ADD CONSTRAINT "Message_owner_id_fkey" FOREIGN KEY (sender_id) REFERENCES public."User"(login) NOT VALID;


--
-- TOC entry 3246 (class 2606 OID 16978)
-- Name: Message none; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Message"
    ADD CONSTRAINT "none" FOREIGN KEY (chat_id) REFERENCES public."Chat"(id) ON DELETE CASCADE;


--
-- TOC entry 3243 (class 2606 OID 17003)
-- Name: Announcement none; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Announcement"
    ADD CONSTRAINT "none" FOREIGN KEY (city_id) REFERENCES public."City"(id) ON DELETE SET NULL NOT VALID;


--
-- TOC entry 3241 (class 2606 OID 17008)
-- Name: User none; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "none" FOREIGN KEY (city_id) REFERENCES public."City"(id) ON DELETE SET NULL NOT VALID;


--
-- TOC entry 3244 (class 2606 OID 16988)
-- Name: Chat pk_announcement_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Chat"
    ADD CONSTRAINT pk_announcement_id FOREIGN KEY (announcement_id) REFERENCES public."Announcement"(id) ON DELETE CASCADE NOT VALID;


-- Completed on 2023-12-01 02:17:03 MSK

--
-- PostgreSQL database dump complete
--

