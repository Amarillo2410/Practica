using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(Infrastructure.Context.AppDbContext))]
    [Migration("20260521170000_LinkedinProfileSchema")]
    public partial class LinkedinProfileSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS public.user_skills CASCADE;
                DROP TABLE IF EXISTS public.skills CASCADE;
                DROP TABLE IF EXISTS public.posts CASCADE;
                DROP TABLE IF EXISTS public.connections CASCADE;
                DROP TABLE IF EXISTS public.education CASCADE;
                DROP TABLE IF EXISTS public.experiences CASCADE;
                DROP TABLE IF EXISTS public.user_sessions CASCADE;
                DROP TABLE IF EXISTS public.user_security CASCADE;
                DROP TABLE IF EXISTS public.job_preferences CASCADE;
                DROP TABLE IF EXISTS public.professional_info CASCADE;
                DROP TABLE IF EXISTS public.user_profiles CASCADE;
                DROP TABLE IF EXISTS public.oauth_accounts CASCADE;
                DROP TABLE IF EXISTS public.external_logins CASCADE;
                DROP TABLE IF EXISTS public.refresh_tokens CASCADE;
                DROP TABLE IF EXISTS public.users CASCADE;

                CREATE TABLE public.users (
                    id uuid NOT NULL,
                    email character varying(150) NOT NULL,
                    phone character varying(30) NULL,
                    password_hash character varying(500) NULL,
                    auth_provider character varying(30) NOT NULL,
                    provider_id character varying(200) NULL,
                    is_email_verified boolean NOT NULL,
                    current_onboarding_step character varying(50) NOT NULL,
                    onboarding_complete boolean NOT NULL DEFAULT false,
                    status character varying(30) NOT NULL,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT pk_users PRIMARY KEY (id)
                );

                CREATE UNIQUE INDEX ix_users_email ON public.users (email);

                CREATE TABLE public.user_profiles (
                    user_id uuid NOT NULL,
                    first_name character varying(100) NULL,
                    last_name character varying(100) NULL,
                    full_name character varying(220) NOT NULL,
                    avatar_url character varying(500) NULL,
                    cover_url character varying(500) NULL,
                    headline character varying(220) NULL,
                    about character varying(4000) NULL,
                    location character varying(220) NULL,
                    country character varying(100) NULL,
                    city character varying(100) NULL,
                    current_company character varying(150) NULL,
                    current_position character varying(150) NULL,
                    public_profile_url character varying(180) NULL,
                    CONSTRAINT pk_user_profiles PRIMARY KEY (user_id),
                    CONSTRAINT fk_user_profiles_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX ix_user_profiles_public_profile_url ON public.user_profiles (public_profile_url);

                CREATE TABLE public.professional_info (
                    user_id uuid NOT NULL,
                    is_student boolean NOT NULL,
                    job_title character varying(150) NULL,
                    company character varying(150) NULL,
                    university character varying(180) NULL,
                    degree character varying(150) NULL,
                    discipline character varying(150) NULL,
                    start_year integer NULL,
                    skills text[] NOT NULL,
                    interests text[] NOT NULL,
                    CONSTRAINT pk_professional_info PRIMARY KEY (user_id),
                    CONSTRAINT fk_professional_info_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE TABLE public.job_preferences (
                    user_id uuid NOT NULL,
                    job_search_status character varying(40) NOT NULL,
                    preferred_titles text[] NOT NULL,
                    preferred_locations text[] NOT NULL,
                    remote_interested boolean NOT NULL,
                    job_alerts_enabled boolean NOT NULL,
                    recruiter_visibility boolean NOT NULL,
                    CONSTRAINT pk_job_preferences PRIMARY KEY (user_id),
                    CONSTRAINT fk_job_preferences_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE TABLE public.user_security (
                    user_id uuid NOT NULL,
                    two_factor_enabled boolean NOT NULL,
                    last_password_change_at timestamp with time zone NULL,
                    CONSTRAINT pk_user_security PRIMARY KEY (user_id),
                    CONSTRAINT fk_user_security_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE TABLE public.oauth_accounts (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    provider character varying(30) NOT NULL,
                    provider_user_id character varying(200) NOT NULL,
                    provider_email character varying(150) NOT NULL,
                    access_token_encrypted character varying(2000) NULL,
                    refresh_token_encrypted character varying(2000) NULL,
                    avatar_from_provider character varying(500) NULL,
                    linked_at timestamp with time zone NOT NULL,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT pk_oauth_accounts PRIMARY KEY (id),
                    CONSTRAINT fk_oauth_accounts_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE INDEX ix_oauth_accounts_user_id ON public.oauth_accounts (user_id);
                CREATE UNIQUE INDEX ix_oauth_accounts_provider_provider_user_id ON public.oauth_accounts (provider, provider_user_id);

                CREATE TABLE public.refresh_tokens (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    token character varying(200) NOT NULL,
                    expires_at timestamp with time zone NOT NULL,
                    revoked_at timestamp with time zone NULL,
                    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
                    CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX ix_refresh_tokens_token ON public.refresh_tokens (token);
                CREATE INDEX ix_refresh_tokens_user_id ON public.refresh_tokens (user_id);

                CREATE TABLE public.user_sessions (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    device character varying(200) NOT NULL,
                    ip character varying(80) NULL,
                    location character varying(180) NULL,
                    created_at timestamp with time zone NOT NULL,
                    last_seen_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_user_sessions PRIMARY KEY (id),
                    CONSTRAINT fk_user_sessions_user_security_user_id FOREIGN KEY (user_id) REFERENCES public.user_security (user_id) ON DELETE CASCADE
                );

                CREATE TABLE public.experiences (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    title character varying(150) NOT NULL,
                    company character varying(150) NOT NULL,
                    location character varying(180) NULL,
                    start_date date NOT NULL,
                    end_date date NULL,
                    currently_working boolean NOT NULL,
                    description character varying(3000) NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_experiences PRIMARY KEY (id),
                    CONSTRAINT fk_experiences_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE INDEX ix_experiences_user_id ON public.experiences (user_id);

                CREATE TABLE public.education (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    school character varying(180) NOT NULL,
                    degree character varying(150) NULL,
                    field_of_study character varying(150) NULL,
                    start_year integer NULL,
                    end_year integer NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_education PRIMARY KEY (id),
                    CONSTRAINT fk_education_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE INDEX ix_education_user_id ON public.education (user_id);

                CREATE TABLE public.skills (
                    id uuid NOT NULL,
                    name character varying(120) NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_skills PRIMARY KEY (id)
                );

                CREATE UNIQUE INDEX ix_skills_name ON public.skills (name);

                CREATE TABLE public.user_skills (
                    user_id uuid NOT NULL,
                    skill_id uuid NOT NULL,
                    endorsement_count integer NOT NULL,
                    CONSTRAINT pk_user_skills PRIMARY KEY (user_id, skill_id),
                    CONSTRAINT fk_user_skills_skills_skill_id FOREIGN KEY (skill_id) REFERENCES public.skills (id) ON DELETE CASCADE,
                    CONSTRAINT fk_user_skills_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE INDEX ix_user_skills_skill_id ON public.user_skills (skill_id);

                CREATE TABLE public.connections (
                    id uuid NOT NULL,
                    requester_id uuid NOT NULL,
                    receiver_id uuid NOT NULL,
                    status character varying(30) NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_connections PRIMARY KEY (id),
                    CONSTRAINT fk_connections_users_receiver_id FOREIGN KEY (receiver_id) REFERENCES public.users (id) ON DELETE RESTRICT,
                    CONSTRAINT fk_connections_users_requester_id FOREIGN KEY (requester_id) REFERENCES public.users (id) ON DELETE RESTRICT
                );

                CREATE INDEX ix_connections_receiver_id ON public.connections (receiver_id);
                CREATE UNIQUE INDEX ix_connections_requester_id_receiver_id ON public.connections (requester_id, receiver_id);

                CREATE TABLE public.posts (
                    id uuid NOT NULL,
                    author_id uuid NOT NULL,
                    content character varying(4000) NOT NULL,
                    media_urls text[] NOT NULL,
                    visibility character varying(30) NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT pk_posts PRIMARY KEY (id),
                    CONSTRAINT fk_posts_users_author_id FOREIGN KEY (author_id) REFERENCES public.users (id) ON DELETE CASCADE
                );

                CREATE INDEX ix_posts_author_id ON public.posts (author_id);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS public.posts CASCADE;
                DROP TABLE IF EXISTS public.connections CASCADE;
                DROP TABLE IF EXISTS public.user_skills CASCADE;
                DROP TABLE IF EXISTS public.skills CASCADE;
                DROP TABLE IF EXISTS public.education CASCADE;
                DROP TABLE IF EXISTS public.experiences CASCADE;
                DROP TABLE IF EXISTS public.user_sessions CASCADE;
                DROP TABLE IF EXISTS public.refresh_tokens CASCADE;
                DROP TABLE IF EXISTS public.oauth_accounts CASCADE;
                DROP TABLE IF EXISTS public.user_security CASCADE;
                DROP TABLE IF EXISTS public.job_preferences CASCADE;
                DROP TABLE IF EXISTS public.professional_info CASCADE;
                DROP TABLE IF EXISTS public.user_profiles CASCADE;
                DROP TABLE IF EXISTS public.users CASCADE;
                """);
        }
    }
}
