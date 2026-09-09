# Automated PR Review

This repo has an automated AI code review set up on pull requests using
OpenAI's Codex GitHub Action. Opening or updating a PR triggers a review
comment; you can also ask follow-up questions by commenting `@codex <question>`
on the PR.

**To validate:** clone this repo, make any change on a branch, and open a PR
against `main`.

**Note:** the review only runs if the PR title contains a specific keyword —
please refer to the email for it.

Thanks for trying it out!

## Sample walkthrough

1. Clone the repo and create a branch, e.g. `git checkout -b test-review`.
2. Make any small change and commit it.
3. Push the branch and open a PR against `main`, including the keyword from
   the email somewhere in the PR title — e.g. `Test change [KEYWORD]`.
4. Within a minute or two, a bot comment appears on the PR with a structured
   review.
5. Push another commit to the same branch. The **same** comment updates in
   place with a fresh review, rather than a new comment being added each
   time.
6. Reply directly in the PR with a comment like
   `@codex why did you flag this?` — Codex will reply with an answer.
7. Open a PR **without** the keyword in the title — no review runs, which
   confirms the opt-in gate is working.
