---
description: "Generate a concise git commit message that follows Conventional Commits and fits git-flow branch context."
name: "Commit Message"
argument-hint: "Describe the intent or constraints for this commit"
agent: "agent"
---
Generate a git commit message for the current repository changes.

Rules:

1. Use Conventional Commits format:
   - `type(scope): subject`
2. `type` must be one of:
   - `feat` `fix` `docs` `refactor` `test` `chore`
3. Infer `scope` from the main module, such as:
   - `sso`, `api`, `recipe`, `docker`, `docs`, `test`
4. `subject` must be written in Chinese, concise, and must not end with `。` or `.`
5. If one commit contains several **related** changes, choose the primary type for the header and use a short body to summarize secondary changes.
6. If the changes are clearly unrelated, output a short Chinese suggestion to split the commit instead of forcing one message.
7. Output only the final commit message unless explicitly asked for alternatives.

Body rules:

- Add one blank line between header and body.
- Each body line should be short and written in Chinese.
- Focus only on the important secondary changes.
- You may use plain list lines like:
  - `- 补充 SSO 处理流程文档`
  - `- 统一 API 错误返回结构`
  - `- 增加错误响应集成测试`

Output pattern:

1. If the changes are related, output:

   `type(scope): subject`

   optional body

2. If the changes are unrelated, output a short Chinese recommendation to split the commit.

Combine any user-provided intent with the actual code changes and generate the most accurate result.
