﻿using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session;
using smartHookah.Services.Person;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Services.Review
{
    public class ReviewService : IReviewService
    {
        private readonly SmartHookahContext db;
        private readonly IPersonService PersonService;

        public ReviewService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            PersonService = personService;
        }


        public async Task<IEnumerable<PlaceReview>> GetPlaceReviews(int id, int pageSize = 10, int page = 0) =>
            await this.db.PlaceReviews
                .Where(a => a.PlaceId == id && !a.Deleted)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();

        public async Task<PlaceReview> AddPlaceReview(PlaceReview review)
        {
            var place = this.db.Places.FindAsync(review.PlaceId);

            if (place == null)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound, "Place not found");
            }

            var author = this.PersonService.GetCurentPerson(db);
            review.AuthorId = author.Id;
            review.Author = author;
            review.PublishDate = DateTime.UtcNow;

            //if (review.SessionReview != null)
            //{
            //    review.SessionReview = await this.AddSessionReviews(review.SessionReview);
            //}

            this.db.PlaceReviews.Add(review);
            this.db.SaveChanges();
            return review;
        }

        public async Task<bool> DeletePlaceReview(int id)
        {
            var review = await this.db.PlaceReviews.FindAsync(id);

            if (review == null)
            {
                throw new ManaException(ErrorCodes.ReviewNotFond, "Place review not found");
            }

            review.Deleted = true;
            this.db.PlaceReviews.AddOrUpdate(review);
            this.db.SaveChanges();

            return true;
        }

        public async Task<IEnumerable<PipeAccessoryReview>> GetPipeAccessoryReviews(int id, int pageSize = 10, int page = 0) =>
            await this.db.PipeAccessoryReviews
                .Where(a => a.AccessorId == id && !a.Deleted)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();

        public async Task<PipeAccessoryReview> AddPipeAccessoryReviews(PipeAccessoryReview review)
        {
            var pipeAccessory = await this.db.PipeAccesories.FindAsync(review.AccessorId);

            if (pipeAccessory == null)
            {
                throw new ManaException(ErrorCodes.AccessoryNotFound, "Accessory not found");
            }

            review.AuthorId = this.PersonService.GetCurentPerson().Id;
            review.PublishDate = DateTime.UtcNow;

            this.db.PipeAccessoryReviews.Add(review);
            this.db.SaveChanges();
            return review;
        }

        public async Task<bool> DeletePipeAccessoryReview(int id)
        {
            var review = await this.db.PipeAccessoryReviews.FindAsync(id);

            if (review == null)
            {
                throw new ManaException(ErrorCodes.AccessoryNotFound, "Accessory review not found");
            }

            review.Deleted = true;
            this.db.PipeAccessoryReviews.AddOrUpdate(review);
            this.db.SaveChanges();

            return true;
        }

        public async Task<IEnumerable<TobaccoReview>> GetTobaccoReviews(int id, SmartHookahContext db, int pageSize = 10, int page = 0) =>
            await (db ?? this.db).TobaccoReviews
                .Where(a => a.AccessorId == id && !a.Deleted)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();


        public async Task<TobaccoReview> AddTobaccoReviews(TobaccoReview review)
        {
            var pipeAccessory = await this.db.PipeAccesories.FindAsync(review.AccessorId);

            if (pipeAccessory == null)
            {
                throw new ManaException(ErrorCodes.AccessoryNotFound, "Accessory not found");
            }

            review.AuthorId = this.PersonService.GetCurentPerson().Id;
            review.PublishDate = DateTime.UtcNow;

            this.db.PipeAccessoryReviews.Add(review);
            this.db.SaveChanges();
            return review;
        }

        public async Task<bool> DeleteTobaccoReviews(int id)
        {
            var review = await this.db.TobaccoReviews.FindAsync(id);

            if (review == null)
            {
                throw new ManaException(ErrorCodes.AccessoryNotFound, "Accessory review not found");
            }

            review.Deleted = true;
            this.db.TobaccoReviews.AddOrUpdate(review);
            this.db.SaveChanges();

            return true;
        }

        public Task<TobaccoReview> GetTobaccoReviewDetail(int reviewId)
        {
            var review = this.db.TobaccoReviews.FindAsync(reviewId);
            if (review == null)
            {
                throw new ManaException(ErrorCodes.ReviewNotFond);
            }
            return review;
        }

        public async Task<IEnumerable<SessionReview>> GetSessionReviews(int id, int pageSize = 10, int page = 0) =>
            await this.db.SessionReviews
                .Where(a => a.SmokeSession != null && a.SmokeSession.Id == id && !a.Deleted)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();

        public async Task<SessionReview> AddSessionReviews(int id, SessionReview review)
        {
            var smokeSession = await this.db.SmokeSessions.FindAsync(id);

            if (smokeSession == null)
            {
                throw new ManaException(ErrorCodes.SessionNotFound, "Session not found");
            }

            review.AuthorId = this.PersonService.GetCurentPerson().Id;
            review.PublishDate = DateTime.UtcNow;
            review.SmokeSession = smokeSession;

            if (review.PlaceReview != null)
            {
                review.PlaceReview = await this.AddPlaceReview(review.PlaceReview);
            }

            if (review.TobaccoReview != null)
            {
                // check , if session have metadata
                if (smokeSession?.MetaData?.TobaccoId == null)
                {
                    // use session data
                    review.nsTaste = review.TobaccoReview.Taste;
                    review.nsSmoke = review.TobaccoReview.Smoke;
                    review.nsStrength = review.TobaccoReview.Strength;
                    review.nsDuration = review.TobaccoReview.Duration;
                    review.TobaccoReview = null;
                }
                else
                {
                    review.TobaccoReview.ReviewedTobaccoId = smokeSession.MetaData.TobaccoId ?? 0;
                    review.TobaccoReview = await this.AddTobaccoReviews(review.TobaccoReview);
                }

            }


            this.db.SessionReviews.Add(review);
            this.db.SaveChanges();
            return review;
        }

        public async Task<bool> DeleteSessionReviews(int id)
        {
            var review = await this.db.SessionReviews.FindAsync(id);

            if (review == null)
            {
                throw new ManaException(ErrorCodes.SessionNotFound, "Session review not found");
            }

            review.Deleted = true;
            if (review.TobaccoReview != null)
                review.TobaccoReview.Deleted = true;

            if (review.PlaceReview != null)
                review.PlaceReview.Deleted = true;

            this.db.SessionReviews.AddOrUpdate(review);
            this.db.SaveChanges();

            return true;
        }

    }
}