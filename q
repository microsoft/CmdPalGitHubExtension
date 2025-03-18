[33mcommit 023f67126a056d1f1994fe82cb8564e9007846f9[m[33m ([m[1;36mHEAD[m[33m -> [m[1;32muser/laurenciha/rate-limits[m[33m, [m[1;31morigin/user/laurenciha/rate-limits[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 20:45:05 2025 -0400

    Modify test to test if search page properly handles rate limit exception

[33mcommit 90f99da9937753452f0d8c03ccd6005cd71ea2d6[m
Merge: c69d179 624e9e0
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 17 14:41:03 2025 -0700

    Merge branch 'dev' into user/laurenciha/rate-limits

[33mcommit c69d17958762796d7be03dbaa9df05ca3c5ad848[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 17 14:10:13 2025 -0700

    Depending on an interface instead of implementation (SOLID principles :D)

[33mcommit 1f11de4dbac248f53a53c881fbdcaf0cc7ec1b1f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 13:43:09 2025 -0400

    Add more (broken) mocks

[33mcommit ea3dd73d80f89099c72b35f161861371c513ef3c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 12:50:24 2025 -0400

    First attempt at rate limit test

[33mcommit 1845279a2932d5b69803bd433e23eac125a42936[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 12:50:08 2025 -0400

    Update SaveSearchForm to use resource strings

[33mcommit 624e9e09ae4cb0d1b4b5ff05dbb3e16dd6a0a543[m[33m ([m[1;31morigin/dev[m[33m, [m[1;32mdev[m[33m)[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 17 09:35:08 2025 -0700

    Changing event types and adding await to submit form method (#170)

[33mcommit 134523b6e30c487d90757308e63c7d7717d2fd61[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 17 09:34:30 2025 -0700

    Fixing some stuff in validator and create helper for requests (#169)

[33mcommit feb98ebcba009f91d59fb41a727c4838f60999ab[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 09:34:33 2025 -0400

    Remove call to public client if the user is not signed in

[33mcommit 083761abf29becf3f4fcd8d902719c8c10ab4f4d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 17 09:15:17 2025 -0400

    Remove instructions to see logs in no items found message

[33mcommit fb9d126cafd99b463ca21fed2692ba3a6428b0f0[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Mar 17 08:48:52 2025 -0400

    Change "search" to "query" in UI strings (#166)
    
    * First round of switching search --> query in UI strings
    
    * Saved GitHub Searches --> Saved GitHub Queries in top level commands
    
    * Search --> Query in SaveSearchTemplate
    
    * Move save button title to resources
    
    * Edit command now shows the correct SaveSearchForm title
    
    * Update resource entries to match naming convention

[33mcommit 55952ee01c6d6e92e30437c3a1a16639fe290848[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Mar 17 08:45:24 2025 -0400

    Add a title and icon to SaveSearchPage (#165)
    
    * Add a title and icon to SaveSearchPage
    
    * Change title parameter to a string instead of passing in the resources object

[33mcommit 66dcca1a125808a278c4a38bb694f71d1fe45de7[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Mar 14 22:39:57 2025 -0400

    Parsing update (#158)
    
    * Fix is:open parsing bug
    
    * Add more GitHub query validation tests
    
    * Add GitHub URL parsing
    
    * Update GitHubQueryValidationTests to include Resources
    
    * ParseSearchFromGitHubUrl --> ParseSearchFromUri

[33mcommit f3305539565357a20e5653ae83cca6b805175523[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 19:36:42 2025 -0700

    Cleaning up Request Options (#156)
    
    * Cleaning request options
    
    * Fixing cancellation token
    
    * Fixing last updated
    
    * Let user determine the state of issues/prs in query
    
    ---------
    
    Co-authored-by: Lauren Ciha <laurenciha@microsoft.com>

[33mcommit ef6a09d493b4e148b3012dbba35198aa4efb6cd9[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 19:06:24 2025 -0700

    Moving code to ensure there is only one page listening to the facade (#157)
    
    * Moving code to ensure there is only one page listening to the facade class. And added tests
    
    * nit fixes

[33mcommit 8b6c90f5f57a8a357b3fb814fec1a10e529738e0[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 16:52:28 2025 -0700

    Several life quality changes that should not change any funcionality (#155)
    
    * Adding more asserts to cache test
    
    * Limiting to 4 labels to not overpopulate the UI
    
    * Changing search to be passed as argument and other changes that I forgot
    
    * Changing everything to UTC to prevent date time conversion bugs
    
    * nits

[33mcommit 9702bf166e6030f976b1245924142ae5beeba1d1[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 12:32:00 2025 -0700

    Removing unnecessary parameter and fixing some tests (#154)

[33mcommit 5e510e873a3d571400bccce7bc5dd337354c1613[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 11:36:50 2025 -0700

    Fixing loading page (#153)
    
    * First changes
    
    * Fixing CacheDataManager Facade tests
    
    * Fixing prs too
    
    * Fixing tests

[33mcommit ec6518ba23bc116fcdb3d22221ba8ea2222a593b[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 10:17:13 2025 -0700

    Using resource localizer for the strings (#151)
    
    * Using resource localizer on search pages
    
    * Adding more strings to pages
    
    * Fixing tests and adding more
    
    * Fixing search page

[33mcommit db45a9481b1d34cc8b2c7b40069315d37eb260a4[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Mar 14 08:55:05 2025 -0700

    Improving lifetime behavior for data objects (#152)
    
    * Removing repository stuff
    
    * Adding delete call
    
    * changing everything to Utc
    
    * Added tests
    
    * Adding more tests!
    
    * Removing unused method

[33mcommit 9a0b9c853ff1c5997e623280649a5fcad388699b[m[33m ([m[1;31morigin/main[m[33m, [m[1;31morigin/HEAD[m[33m, [m[1;32mmain[m[33m)[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 18:41:28 2025 -0700

    Fixing methods that were not deleted

[33mcommit 972ba1654256ea146e43babd630d124310151cc7[m
Merge: 5433620 7535dff
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 18:32:29 2025 -0700

    Merge branch 'dev'

[33mcommit 7535dffc9a12fef6d2db12db2a4d25edc1512090[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Mar 12 21:26:07 2025 -0400

    Bump to 0.0.5 (#150)

[33mcommit 72fd2d36fceb393a2368497e4e07a0cfb91d7b4b[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Mar 12 21:05:51 2025 -0400

    Add default searches upon login (#149)
    
    * Add default searches into GitHubExtensionCommandsProvider
    
    * Add default search validation and remove awaits
    
    * Put PersistentManager methods on the same thread

[33mcommit 2a4186d2d18a0b04dc254c65d0e3f3696b1eac51[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Mar 12 15:58:52 2025 -0700

    Merge pull request #148 from microsoft/user/felipeda/strings
    
    Adding strings to resources

[33mcommit e3543ffc6b71193093a17882649d8e172e390dcd[m
Merge: a309372 e721960
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Mar 12 15:06:20 2025 -0700

    Merge pull request #147 from microsoft/user/felipeda/1espipeline
    
    Implementing 1ES M365 template on build pipeline

[33mcommit a309372891f5b3cd12b0408506db77fedf238fb1[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Mar 12 18:05:34 2025 -0400

    Add UI for pinning searches to the top level (#146)
    
    * Combined search is working, copy PR source branch is not (as expected for first try)
    
    * Add fix for copying source branch
    
    * Make icon for combined search GitHub logo (this could change later)
    
    * Add pin to top level commands checkbox in SaveSearch template
    
    * Add logic to parse IsTopLevel from template and add to SearchCandidate
    
    * Checkpoint
    
    * Checkpoint: everything compiles
    
    * Adding more SaveSearchFormTests
    
    * Update test to use SubmitForm
    
    * Actually fix build errors
    
    * Attempt SignInPageTest
    
    * Delete broken test
    
    * Add new function call
    
    * Work in progress
    
    * Update CommandsProvider to add TopLevel commands
    
    * Did some stuff
    
    * Add test to verify that unchecking the Top Level command box removes search from the top level commands
    
    * Top level commands updates when the IsTopLevel box is unchecked
    
    * Removed top-level commands now update in the top level
    
    * Add more top level search tests
    
    * Attempt to add tests
    
    * Remove event checking from tests
    
    * Move checkbox text into checkbox label instead of putting it in a separate column
    
    * More fixes in TopLevelSearchesTest and SaveSearchFormTest
    
    * Set IsTopLevelChecked upon initialization instead of during template loading
    
    * Remove unnecessary parameter in GitHubExtensionCommandsProvider and update call to GetTopLevelSearchCommands()
    
    * Update TopLevelSearchesTest to use randomized paths for the PersistentDataManager
    
    * Remove virtual from SavedSearchPage's OnSearchSaved signature
    
    * Make PersistentDataManagerTestsSetup static
    
    ---------
    
    Co-authored-by: guimafelipe <guima.felipec@gmail.com>

[33mcommit e72196022c30bb2754f24164239286bfb4cfa882[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 11:53:01 2025 -0700

    Separating artifact names

[33mcommit 56d00f10fd3fcde6cef74ff9d0feb57c801987be[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 10:49:40 2025 -0700

    Fixing path

[33mcommit 6b13aa8b1503dcebfb4c9356bd7371bf51a3e134[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 10:48:47 2025 -0700

    Changing to official

[33mcommit 61d021f4ff61b0bb93081adec124d408cd68a59d[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 10:45:45 2025 -0700

    Removing conditional

[33mcommit 9e3fc32bf6c8812891dde9ffbd443db8c2690a7c[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Mar 12 10:43:22 2025 -0700

    editing pipeline

[33mcommit 33c173fa9c80827d8fc5d82a5fb27776086fd1e6[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Tue Mar 11 09:53:50 2025 -0700

    Adding is top level check (#143)
    
    * Adding method to interface
    
    * Adding tests

[33mcommit 345468c8d2e2f4c7b93f61addd5b0c649c52c935[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Mar 11 11:02:01 2025 -0400

    Add combined search UI (#142)
    
    * Combined search is working, copy PR source branch is not (as expected for first try)
    
    * Add fix for copying source branch
    
    * Make icon for combined search GitHub logo (this could change later)
    
    * Fixing tests and decorator injection
    
    ---------
    
    Co-authored-by: guimafelipe <guima.felipec@gmail.com>

[33mcommit 410f13c498dc8914b51407dd8c2c04fa681f1bed[m
Merge: 669d689 39c0446
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 10 17:04:15 2025 -0700

    Merge pull request #139 from microsoft/user/felipeda/toplevelsearches
    
    Adding support to top level searches

[33mcommit 669d68973b603023040bef8d0f3db244a1ccdf8f[m
Merge: 9d88789 803eb6e
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 10 17:03:47 2025 -0700

    Merge pull request #140 from microsoft/user/felipeda/issuesandprs
    
    Adding support for search for Issues and PRs

[33mcommit 9d887896be23fd258d3e18844ce4c057dc4df329[m
Merge: e03ceb3 6ac385b
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Mar 10 19:58:32 2025 -0400

    Merge pull request #138 from microsoft/user/laurenciha/remove-search-survey
    
    Remove add search by survey (aka "full form")

[33mcommit 6ac385b0a66fba80d6df41046a989a291f362b04[m[33m ([m[1;31morigin/user/laurenciha/remove-search-survey[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 19:43:06 2025 -0400

    Nit fixes

[33mcommit 803eb6e011a9dbb1a2c2cb9ff37b6f18eca8c5fd[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 16:31:40 2025 -0700

    Fixing typo error 2

[33mcommit 9b0431441d483d7a5f6c74c3e43c784e357c89a7[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 16:30:13 2025 -0700

    fixing typo error

[33mcommit c8ca24ded9c0157846e687ebd1e55cab95eeb8c0[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 16:28:40 2025 -0700

    Reverting schema version thing

[33mcommit 142b593f558d7964f5e88cf1b8a1df559a455dc3[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 16:25:11 2025 -0700

    Adding tests

[33mcommit f99f980413721669bf34bfab4e7dfd1edab61e88[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 15:18:19 2025 -0700

    Adding support for querying issues and pull requests

[33mcommit 39c04461275fd156912bf7c31459b5a1a46715ec[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 13:40:09 2025 -0700

    Adding support to top level searches

[33mcommit 975ba75c8d3881bbca3e6586a68b7a3efbf802ff[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 14:54:06 2025 -0400

    Update and rename SavedSearchesPageTest

[33mcommit 746def562102c7d013a5d4464503dd8d5311b878[m
Merge: 32ff6fc e03ceb3
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 14:46:06 2025 -0400

    Merge upstream dev

[33mcommit e03ceb39bfe95d0051fde5db69d8bc6b67bbca18[m
Merge: 10be9ec 2efe09c
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 10 10:14:35 2025 -0700

    Merge pull request #134 from microsoft/user/felipeda/initialpagestests
    
    Adding pages tests

[33mcommit 32ff6fc1d8a08a78b5f6a379fe41739220e56894[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 12:54:40 2025 -0400

    Remove long unused StateJsonPath() from GitHubHelper

[33mcommit 5b4b0d5c2ff36670d5de76849b12114a1b82f8cf[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 12:51:38 2025 -0400

    Remove add search survey

[33mcommit 2efe09cca306aca91481bcae67b27564159fc022[m
Merge: e7c7ed4 10be9ec
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Mon Mar 10 09:08:24 2025 -0700

    Merge branch 'dev' into user/felipeda/initialpagestests

[33mcommit 10be9ec76eb6ff8ed41cb9f1eae376947b062800[m
Merge: 0957e0a 50aaa9b
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 10 09:02:34 2025 -0700

    Merge pull request #133 from microsoft/user/felipeda/addsearchdatamodelreadandwritetest
    
    Adding tests for Search data model object

[33mcommit 0957e0ac27a407a6a189a1f3b9a639b57b98a62c[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Mar 10 09:01:50 2025 -0700

    Quality of life changes for cache manager (#132)

[33mcommit e7c7ed43c1942decf5f4f0a738d5c3f9746ec45e[m[33m ([m[1;31morigin/user/felipeda/initialpagestests[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Mar 10 11:21:22 2025 -0400

    Add test that OnSearchRemove has the correct number of list items after execution

[33mcommit ca7e61eb2ed76c1ab48b2c18656dbb30fa11d290[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Fri Feb 28 17:52:01 2025 -0800

    Adding pages tests

[33mcommit 50aaa9ba244b5193622dcc6577b78d802d189211[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Fri Feb 28 16:29:39 2025 -0800

    Fixing order of assert

[33mcommit b7e1baa6bd0c794e969e33f62d25df5d207f85d0[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Fri Feb 28 16:27:06 2025 -0800

    Adding tests

[33mcommit bba7264f232c3e4381dd9d4eddfea00d4531fa27[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 15:01:06 2025 -0800

    Adding persistent data tests. Separating cache manager tests. (#122)
    
    * Adding persistent data tests. Separating cache manager tests.
    
    * Fixing names
    
    * Fixing namespace
    
    * Abstracting the validator

[33mcommit 79017a6719c0d7a02cafbcaaba3a177975869a1e[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 14:00:22 2025 -0800

    Create CI.yml (#131)
    
    * Create CI.yml
    
    * Update .github/workflows/CI.yml
    
    Co-authored-by: Copilot <175728472+Copilot@users.noreply.github.com>
    
    ---------
    
    Co-authored-by: Copilot <175728472+Copilot@users.noreply.github.com>

[33mcommit 4ba5b8cdcfb5250b1aabead0fdc0da7f531d28b1[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Feb 28 16:56:30 2025 -0500

    Update namespaces (#123)
    
    * Update namespaces
    
    * Correct namespace for CacheDataManagerFacade
    
    * GitHubExtension.DataManager.CacheManager.CacheManagerStates; --> GitHubExtension.DataManager.CacheManager;
    
    * GitHubExtension.Test/DataStore --> GitHubExtension.Test/DataStoreTests
    
    * Remove ...Controls.Pages from Controls that aren't in the pages folder
    
    * ...Pages.SearchPages --> ...Pages
    
    * Fix classes from previous changes
    
    * DataManager.DataManager --> DataManager

[33mcommit bd62931a0cac1274ffc806af6670fea1e01dff60[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Fri Feb 28 09:00:37 2025 -0800

    Hot fixes

[33mcommit 3ab3742e5f3591b007a6592647d9ccc19136ed6a[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 08:41:17 2025 -0800

    Removing cache responsibility from page into cache manager (#119)
    
    * Removing cache responsibility from page
    
    * Small fixes

[33mcommit dc31fc2fea4c54277ab7b10dbd671392f85d50e4[m
Merge: 3319dcd 0f27ce4
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 08:38:48 2025 -0800

    Merge pull request #116 from microsoft/user/felipeda/sourcebranch
    
    Adding source branch decorator to IPullRequest instances

[33mcommit 3319dcd7331c3af588d14e0416d17cdb36d283db[m
Merge: f1fa365 427324d
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 08:38:18 2025 -0800

    Merge pull request #117 from microsoft/user/felipeda/ideveloperid
    
    Using IDeveloperId instead of implementation

[33mcommit f1fa365a4b8fb33700b084799d2a9921d9b68470[m
Merge: bbe617f 06b918f
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 28 08:37:46 2025 -0800

    Merge pull request #118 from microsoft/user/felipeda/iissueipr
    
    Making IPullRequest implement IIssue

[33mcommit 0f27ce4fe5aa8ff04a4d3150a62499dfdc08d301[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 27 20:32:16 2025 -0800

    Renaming file

[33mcommit 2e83eccf08c8d512e9f58c2d23eed7cc1a0d1b61[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 27 17:50:36 2025 -0800

    Adding pull request source branch decorator

[33mcommit 06b918ff0a485b5114237fead8fdd0841eccbf03[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 27 15:59:56 2025 -0800

    Making IPullRequest implement IIssue

[33mcommit 427324d51204483f052ec65a38efcf7346799872[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 27 15:54:06 2025 -0800

    Using IDeveloperId instead of implementation

[33mcommit bbe617f5ea69e48b0eb1124b6a9420f113041644[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Thu Feb 27 11:53:23 2025 -0800

    Using Dependency Injection (#110)
    
    * Refactoring UI layer to use DI
    
    * Refactoring data code.
    
    * Fixing unit tests
    
    * Moving stuff around
    
    * Added CacheManager tests
    
    * Reformat async code to remove Wait()
    
    * Add name to EditSearchPage command
    
    * Removing sleep from tests
    
    ---------
    
    Co-authored-by: Lauren Ciha <laurenciha@microsoft.com>

[33mcommit 54336207f8b160981bb4c48086ac811f81fe545d[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Feb 26 15:37:27 2025 -0500

    [0.0.4] Add labels (#109)
    
    * UI updates to SavedSearchesPage (#100)
    
    * Rename and reorder commands for an empty SavedSearchesPage
    
    * Reduce duplication of addSearchListItem and addSearchFullFormListItem
    
    * Remove unused OnSearchSaving event handler from SavedSearchesPage
    
    * Add SearchString as SavedSearch item subtitle
    
    * Update SavedSearchesPage icon and title
    
    * Fix edit search bug (#99)
    
    * Line ending update
    
    * When editing a search, remove old search only after new one is added
    
    * Line ending update and change from using a var check to a null-conditional operator (?) in HandleDataManagerUpdate() (#101)
    
    * Fix Sign out form's indefinite loading (#102)
    
    * Line ending updates
    
    * Add GitHubForm events to auth flows
    
    * Bump to 0.0.4 (#105)
    
    * Display labels for issues and pull requests (#108)
    
    * Add one issue as a tag
    
    * Move tag generation to SearchPage.cs and add background color
    
    * Add function to determine white or black font color
    
    * Add labels to PR objects

[33mcommit 9937abf723aa66be647ceac225d2f57babb6f09b[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Feb 26 12:45:59 2025 -0500

    Display labels for issues and pull requests (#108)
    
    * Add one issue as a tag
    
    * Move tag generation to SearchPage.cs and add background color
    
    * Add function to determine white or black font color
    
    * Add labels to PR objects

[33mcommit e7b6abddca7c35b9ee5f678f39b2ccfa74256893[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:57:55 2025 -0500

    Bump version to 0.0.4 (#106)
    
    * UI updates to SavedSearchesPage (#100)
    
    * Rename and reorder commands for an empty SavedSearchesPage
    
    * Reduce duplication of addSearchListItem and addSearchFullFormListItem
    
    * Remove unused OnSearchSaving event handler from SavedSearchesPage
    
    * Add SearchString as SavedSearch item subtitle
    
    * Update SavedSearchesPage icon and title
    
    * Fix edit search bug (#99)
    
    * Line ending update
    
    * When editing a search, remove old search only after new one is added
    
    * Line ending update and change from using a var check to a null-conditional operator (?) in HandleDataManagerUpdate() (#101)
    
    * Fix Sign out form's indefinite loading (#102)
    
    * Line ending updates
    
    * Add GitHubForm events to auth flows
    
    * Bump to 0.0.4 (#105)

[33mcommit bdb9b1781a6ea65bcd53c45705727571b4b109a9[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:57:10 2025 -0500

    Bump to 0.0.4 (#105)

[33mcommit 5e39dff969ae0580a7b5cde543fd71fbbb0c70a0[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:09:35 2025 -0500

    UI updates and bug fixes (#104)
    
    * UI updates to SavedSearchesPage (#100)
    
    * Rename and reorder commands for an empty SavedSearchesPage
    
    * Reduce duplication of addSearchListItem and addSearchFullFormListItem
    
    * Remove unused OnSearchSaving event handler from SavedSearchesPage
    
    * Add SearchString as SavedSearch item subtitle
    
    * Update SavedSearchesPage icon and title
    
    * Fix edit search bug (#99)
    
    * Line ending update
    
    * When editing a search, remove old search only after new one is added
    
    * Line ending update and change from using a var check to a null-conditional operator (?) in HandleDataManagerUpdate() (#101)
    
    * Fix Sign out form's indefinite loading (#102)
    
    * Line ending updates
    
    * Add GitHubForm events to auth flows

[33mcommit 3cd2aff4ec30fb4c745efad6d99459b5cdb202c8[m
Merge: 4ee1409 a07cba2
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:05:54 2025 -0500

    Merge branch 'main' into dev

[33mcommit a07cba25a47911f8eeadcf73f112e2bb693c04f6[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:04:15 2025 -0500

    User/laurenciha/remove search issues and prs (#103)
    
    * UI updates to SavedSearchesPage (#100)
    
    * Rename and reorder commands for an empty SavedSearchesPage
    
    * Reduce duplication of addSearchListItem and addSearchFullFormListItem
    
    * Remove unused OnSearchSaving event handler from SavedSearchesPage
    
    * Add SearchString as SavedSearch item subtitle
    
    * Update SavedSearchesPage icon and title
    
    * Fix edit search bug (#99)
    
    * Line ending update
    
    * When editing a search, remove old search only after new one is added
    
    * Line ending update and change from using a var check to a null-conditional operator (?) in HandleDataManagerUpdate() (#101)
    
    * Update line endings
    
    * Remove SearchIssuesPage and SearchPullRequestsPage from top-level commands

[33mcommit 4ee1409069bf7df3d4fc579dd65436b29fdc83fd[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 15:01:35 2025 -0500

    Fix Sign out form's indefinite loading (#102)
    
    * Line ending updates
    
    * Add GitHubForm events to auth flows

[33mcommit cb33d5533384b3a13c6c91078aa65b8cbc6594f9[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 13:19:32 2025 -0500

    Line ending update and change from using a var check to a null-conditional operator (?) in HandleDataManagerUpdate() (#101)

[33mcommit e8d37410336eebe1125b7aa7f753379bbbac8fa6[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 13:12:25 2025 -0500

    Fix edit search bug (#99)
    
    * Line ending update
    
    * When editing a search, remove old search only after new one is added

[33mcommit d59dbbec469c0d1e09544e624db0628796935131[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Tue Feb 25 13:11:59 2025 -0500

    UI updates to SavedSearchesPage (#100)
    
    * Rename and reorder commands for an empty SavedSearchesPage
    
    * Reduce duplication of addSearchListItem and addSearchFullFormListItem
    
    * Remove unused OnSearchSaving event handler from SavedSearchesPage
    
    * Add SearchString as SavedSearch item subtitle
    
    * Update SavedSearchesPage icon and title

[33mcommit 196775d9f129feb70e101e5126f94fa2795261ef[m
Merge: ec7e4c9 c8a47f0
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Feb 24 18:33:48 2025 -0500

    Merge pull request #96 from microsoft/dev
    
    Code was reviewed in previous PRs into dev

[33mcommit c8a47f045d7c456b2d5bed2114a098a83a60fe10[m
Merge: 3ae805d 0b325b3
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Feb 24 18:32:43 2025 -0500

    Merge pull request #95 from microsoft/user/laurenciha/sdk0.0.7
    
    Update Microsoft.CommandPalette.Extensions to 0.0.7

[33mcommit 0b325b314b01724400f8ea90978ad196715dc120[m[33m ([m[1;31morigin/user/laurenciha/sdk0.0.7[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 18:31:13 2025 -0500

    MarkdownPage -> ContentPage

[33mcommit 22676f8a8ddf22a519508c062b395af4e0b1b3f3[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 18:01:55 2025 -0500

    Update Microsoft.CmdPal.Extensions.SDK.0.0.7.0

[33mcommit a046987d0007ec972a91af48f750306b8aae098c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 17:54:59 2025 -0500

    FormPage -> ContentPage and Form -> FormContent

[33mcommit 3ae805de15fffba27f0914025b112cd8547c9cfb[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 17:28:49 2025 -0500

    Delete duplicate SearchPage class

[33mcommit 527df1e173302fb41ede903a6b051f6d0ecd1f3f[m
Merge: 8aa65ab bf7462b
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Feb 24 16:23:09 2025 -0500

    Merge pull request #92 from microsoft/user/laurenciha/episode-ii
    
    Page Refactoring (Episode II)

[33mcommit bf7462bd59c5d5b7a709dc1a9fe30bef42b03b93[m[33m ([m[1;31morigin/user/laurenciha/episode-ii[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 13:26:40 2025 -0500

    Remove AddRepo form, page, and template

[33mcommit 89749bf0ea4c1988c4913979ab8aa5ab63593799[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 13:23:31 2025 -0500

    Remove SampleGitHubForm and SampleGitHubFormPage

[33mcommit ff0d793ea7ebd54c4bf4c96622d4b179faa310c2[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 11:51:09 2025 -0500

    Update AddRepoPage constructor to have more parameters

[33mcommit 383188253dbaae835d06bc1be7f284b284c7cabd[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 11:46:17 2025 -0500

    Looks bigger than it is: refactored the AuthForm code

[33mcommit 00da4241ec9d62f6bd6d23ea5b6fd58754cbd25b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 11:25:22 2025 -0500

    Update SaveSearchPage constructors so all fields are parameters

[33mcommit 1741b9f4cfd7a70c0ea089a677768ce3fdc3a15a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 11:07:44 2025 -0500

    Remove previous EditSearchPage constructor

[33mcommit 7bbe3715b323ab5414a233830f96f1819de17ec7[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 24 11:06:18 2025 -0500

    Add constructor with all parameters needed

[33mcommit 8aa65ab05bd43e37740d82917d4e21daead9ebd7[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Feb 22 10:31:43 2025 -0800

    Adding initial tests (#89)
    
    * Adding and building test project
    
    * Testing passing
    
    * Editing pipeline
    
    * Adding test script
    
    * Adding test output variable

[33mcommit 65294f29f2201a9982bc73ead802fa3c0cbc94f4[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Sat Feb 22 10:53:56 2025 -0500

    SaveSearchPage is fully a GitHubFormPage, EditSearchPage is now a GitHubFormPage

[33mcommit 5a9a7e0d11e637f2f123467849e1818acff71cdf[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Sat Feb 22 10:38:24 2025 -0500

    SaveSearchPage is now a GitHubFormPage

[33mcommit ec7e4c97e07daa336c0130afa4da242a26dce5b0[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Fri Feb 21 15:45:03 2025 -0800

    Revert "Update azure-pipelines.yml for Azure Pipelines"
    
    This reverts commit a0b576ad4a359ad41ae569fec217404f65509b62.

[33mcommit a0b576ad4a359ad41ae569fec217404f65509b62[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 21 15:41:06 2025 -0800

    Update azure-pipelines.yml for Azure Pipelines

[33mcommit aa786be2ecfdc569ddbdfe0f76904a657b18aca9[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 17:31:45 2025 -0500

    Syncing AddRepoForm and page

[33mcommit 54bd48ee1c3db74b21ed3f122658cd2a3a23ab1d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 17:31:27 2025 -0500

    Line ending update

[33mcommit 1966af697ba88732fed316c60892031e64ff5afc[m
Merge: d9ad395 291f0f9
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Feb 21 17:07:00 2025 -0500

    Merge pull request #88 from microsoft/user/laurenciha/page-refactor
    
    FormPage refactoring (Episode I)

[33mcommit 663968a7e59c59209d1a84768c71de7da95ceff8[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 17:03:29 2025 -0500

    AddRepoForm is now a GitHubForm

[33mcommit 291f0f94224e5e417013878a94774f9d5802a290[m[33m ([m[1;31morigin/user/laurenciha/page-refactor[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 16:32:50 2025 -0500

    Add more error handling code (not fully functional)

[33mcommit 10b4759d6e85d0cf675de63c6cd96f581c61b81d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 16:04:46 2025 -0500

    SignOutForm and Page and are now a GitHubForm and GitHubPage

[33mcommit 48f44f23dab4b1c20a09abfbdf57b8d57a7d3813[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 15:35:20 2025 -0500

    GitHubAuthPage is now a GitHubFormPage

[33mcommit bb1f86303e7e3a7cff54315ee6553ad0fd5e413c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 15:16:46 2025 -0500

    Make GitHubAuthForm a GitHubForm

[33mcommit 652e1b3d4ce91eae2735d63469959ebab8eb60d0[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 14:57:44 2025 -0500

    GitHubSignInTemplate.json --> AuthTemplate.json and remove reference to SaveSearchData.json (which no longer exists)

[33mcommit d9ad3958683be8195846aa41f2b33afa44698a55[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 21 10:31:41 2025 -0800

    Some magic (#87)

[33mcommit 6aa365b5966a9e64b21ccd3f1ae3c995ad0ae227[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 13:14:54 2025 -0500

    Make SuccessMessage and ErrorMessage properties modifiable in child GitHubFormPage classes

[33mcommit 946f69e756dc3f9062fd57336cfcddab6f808adb[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 13:00:01 2025 -0500

    Make StatusMessage, Form, and event paramenters child class specific

[33mcommit f4c464cb1e0d4561e8601f8f9506f45fd611aa60[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 12:26:10 2025 -0500

    Add samples to confirm code's working

[33mcommit a42d16b884e70518d1489e289393699e0c04a114[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 21 09:06:47 2025 -0800

    Adding cooldown to page (#86)
    
    * Adding cooldown to search page
    
    * Removing bool flag
    
    * Adding lock

[33mcommit af5fd00a8850107a74e6a747244ee0790b3d2a5e[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 11:22:34 2025 -0500

    Add first draft of FormPage and IGitHubPage

[33mcommit 662e0806c69159354557a1b1cd6d947fe1ba7092[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 10:47:39 2025 -0500

    Add initial GitHubForm template with FormSubmitEventArgs

[33mcommit 30b097d6812b65b2bd43668e0b3cf9918c52a99d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:46:16 2025 -0500

    Remove SearchMentionsPage (no longer used)

[33mcommit dc5d0a96392a04654379bb9c7bde2ab401ea7adb[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:45:10 2025 -0500

    Remove SignOutCommand (no longer used)

[33mcommit a098ac02e33fd08d95798b31df127c76e2ea286e[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:36:51 2025 -0500

    Remove search repositories page (new version is coming)

[33mcommit 2b3cc00bab2c9e10c30d28a5e71141af7c7d5d95[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:36:20 2025 -0500

    remove search releases (outdated)

[33mcommit 1d1a7b8b96a56dea5635883926c7831c9429b750[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:29:54 2025 -0500

    Remove PAT support (we're using OAuth now)

[33mcommit 2c511af4df154b1483ce5c7e06a4b2f5a39451a4[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 21 09:25:56 2025 -0500

    Remove AddOrganization classes

[33mcommit bd15a59d0983ee522f8680bafc051f9ab8a67df5[m
Merge: 35bc449 aa5bd7e
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 20 15:54:56 2025 -0800

    Merge branch 'user/laurenciha/saved-searches' into dev

[33mcommit aa5bd7e9e537d4946f606e5031cc8c6e11febad1[m[33m ([m[1;31morigin/user/laurenciha/saved-searches[m[33m)[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 20 15:52:43 2025 -0800

    small fixes

[33mcommit 35bc449fe6f3bfb2e334ffa8a5112f6bc06fff0f[m
Merge: 0c65965 bbeb051
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 20 18:08:11 2025 -0500

    Merge pull request #57 from microsoft/user/laurenciha/quick-fixes
    
    User/laurenciha/quick fixes

[33mcommit 20daf8644d85dca7cd7d7d4aa3b49929abbc10bc[m
Merge: b2cf5e2 0a06d82
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 20 17:39:00 2025 -0500

    Merge pull request #71 from microsoft/user/laurenciha/edit-search
    
    Add ability to remove and edit saved searches

[33mcommit 0a06d823d419d8d17c8bbd53b7eafaab1062cbb5[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 20 17:38:36 2025 -0500

    Update GitHubExtension/Helpers/SearchHelper.cs
    
    Co-authored-by: Copilot <175728472+Copilot@users.noreply.github.com>

[33mcommit d01331eb73f7fa82f5b5227017b3d68c73840eae[m
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 20 17:38:18 2025 -0500

    Add false case to OnSavedSearch
    
    Co-authored-by: Copilot <175728472+Copilot@users.noreply.github.com>

[33mcommit 9e8a5fe13c4e1be2da2f73f3723e1a5f708a76a2[m[33m ([m[1;31morigin/user/laurenciha/edit-search[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 17:04:24 2025 -0500

    Add icons for SavedSearchesPage, SaveSearchPage, RemoveSaveSearch, and EditSavedSearch

[33mcommit f13c9383e6da186406e4ba537d8179909cae9e3a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 16:44:56 2025 -0500

    Add edit saved search

[33mcommit 253736d952f806c905b1cec5a3bfd131f0a6cd03[m
Merge: 1b2b7f9 639d4d3
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 15:13:18 2025 -0500

    Merge branch 'user/laurenciha/remove-saved-search' into user/laurenciha/edit-search

[33mcommit 639d4d3f16f0565d171f4eb7da986526918cefeb[m[33m ([m[1;31morigin/user/laurenciha/remove-saved-search[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 15:05:13 2025 -0500

    Add error propagation to remove search

[33mcommit 37fef41632bd5032fd69b40ee32cf7d0fbd40ac1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 14:37:59 2025 -0500

    Make add and remove functions return Task

[33mcommit 3e0d1a95297ef5efe4692955c64c43d4828994c8[m[33m ([m[1;31morigin/user/felipeda/fixingremove[m[33m)[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 20 11:00:18 2025 -0800

    Fixing logger context

[33mcommit a8766aff334a05cc040e3116493718095472ed64[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Thu Feb 20 10:55:20 2025 -0800

    Changing remove to explicit SQL queries

[33mcommit 1b2b7f905d0c2067205744f4ebb0169031183e82[m
Merge: a401768 177f88b
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 13:29:24 2025 -0500

    Merge branch 'user/laurenciha/remove-saved-search' into user/laurenciha/edit-search

[33mcommit 177f88bd19d28d6e9304df5af6d93f30dc5e02b1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 13:25:50 2025 -0500

    Line ending update

[33mcommit bf218fe4464fc5627cddafee4b8d236ed396282b[m
Merge: ada37af b2cf5e2
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 11:56:36 2025 -0500

    Merge branch 'user/laurenciha/saved-searches' into user/laurenciha/remove-saved-search

[33mcommit b2cf5e2f270aa2fe899194982832865ff14bfe28[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Thu Feb 20 08:42:35 2025 -0800

    Implementing search pull requests (#69)
    
    * Big refactoring of cache manager
    
    * Implementing search pull requests
    
    * Refactoring search page and adding PR queries

[33mcommit 9850456222d271c68e32f1061d98903ae7a6187a[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Thu Feb 20 08:38:25 2025 -0800

    Big refactoring of cache manager (#68)

[33mcommit a40176820df1f7490767588758ce729362415fa8[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 20 09:19:18 2025 -0500

    Line ending update

[33mcommit 8e768cea895f62bff70adc8b9539d00a28884b95[m
Merge: f917728 ada37af
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:43:06 2025 -0500

    Merge branch 'user/laurenciha/remove-saved-search' into user/laurenciha/edit-search

[33mcommit ada37afdc002aefa7e53c534133c53b2d83f748d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:41:53 2025 -0500

    Add loading animation on remove command

[33mcommit c6faa521581dbc114927e74cd0dc90d95dbe9194[m
Merge: 8b53b63 fb67e21
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:41:22 2025 -0500

    Merge user/laurenciha/saved-queries

[33mcommit fb67e210da5f75952661073c1333c375f8d3ec65[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:20:49 2025 -0500

    Add PullRequests to IconDictionary

[33mcommit f917728bac29937f6b555defc4618c19e3ce5227[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:03:18 2025 -0500

    Add PullRequests string to IconDictionary

[33mcommit 5837764a4eb8a6ee418f66b2f1b353bd518f45cf[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 17:02:54 2025 -0500

    Delay success message until search is added to database

[33mcommit 183694809232f8f19d0bfae39aa74bace1fd8aae[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 16:45:04 2025 -0500

    Update title in SaveSearchForm and delete SaveSearchData.json

[33mcommit 9f363ee11992b3b8c44d2771fe0e46c37e16439b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 16:26:57 2025 -0500

    Add calls to database to replace former search with edited one

[33mcommit 79b9fd546c1e9cedfcbfed98c03f8bfa99ee572f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 16:15:37 2025 -0500

    Add savedSearch's title to page template

[33mcommit 9ac4ef1b3e0146c8edf4713a94f27efe9c73a984[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Feb 19 13:14:23 2025 -0800

    Cleaning data objects (#63)
    
    * Removing unused methods from data manager
    
    * Removing checks from pull requests
    
    * Removing enums related to checks for pull requests
    
    * Cleaning schema
    
    * Finishing cleaning data schema
    
    * Finishing removing PR status
    
    * lil
    
    * Fixing merge issues
    
    * fixing after merge
    
    ---------
    
    Co-authored-by: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>

[33mcommit 64da3e4c6d08a27af3d3ecaeea7d4de99234d8af[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 16:10:52 2025 -0500

    savedSearch's SearchString now populates edit template

[33mcommit 078c93b4d8043ee2eeca86f6bf56950bd72bf9ed[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 16:02:10 2025 -0500

    Add savedSearch variable to SaveSearchForm.cs (empty strings work)

[33mcommit 17029d8fcf5a12cec80f6c1ea367b7825352582a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 15:58:55 2025 -0500

    Add variable SavedSearch to template

[33mcommit 4b342bd256d0915ed8e83b08cee963c07000f668[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 15:55:12 2025 -0500

    Add shell of edit search page and template

[33mcommit aef8cfb9f31768e808821f5090921957db5ca7e7[m
Merge: 91e3dfa 07c3430
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Feb 19 13:26:44 2025 -0500

    Merge pull request #62 from microsoft/user/felipeda/refactoringsearchtype
    
    Refactoring searchtype

[33mcommit 07c343070d2a69d42d01976423897a6290e3638e[m
Author: guimafelipe <guima.felipec@gmail.com>
Date:   Wed Feb 19 10:19:58 2025 -0800

    Refactoring searchtype

[33mcommit 8b53b630f3cce57b8d9073e58caaf813718e43a3[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 13:05:49 2025 -0500

    Update line endings and add logs for Search.Remove

[33mcommit 61265e5a4a44b2cdc1d5036480f9555f5f1dccff[m
Merge: 985a74b 91e3dfa
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 11:33:01 2025 -0500

    Merge branch 'user/laurenciha/saved-searches' into user/laurenciha/remove-saved-search

[33mcommit 91e3dfa594fd8dfc49d158f430f0a0eab8b34d15[m
Author: guimafelipe <felipeda@microsoft.com>
Date:   Wed Feb 19 08:20:12 2025 -0800

    Fixing null reference and limiting request on searchpage

[33mcommit 985a74b5abee7297eb9c3350ab4ddacd01d98c5a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 11:01:46 2025 -0500

    Flag where deletion isn't working

[33mcommit ea1ea9a01e930446a69d62851d3a3aee76d6af1b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 10:50:13 2025 -0500

    Shell of remove search command

[33mcommit ccca40faba7b639e7f4d5fcec70f1b53294608e1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 10:13:01 2025 -0500

    Update line endings solution

[33mcommit 2e68762a9b25a6ab45b846c572bf5bc55c19c3e3[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 09:44:32 2025 -0500

    Update line endings in Search and PersistentDataManager

[33mcommit a3b0563d5488a5ab8c870ad7ea9bf526f059c3a0[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 19 09:27:29 2025 -0500

    Update line endings in GitHubDataManager

[33mcommit e91c26a124d19ec76890d096cbf844fa52b5ad5a[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Tue Feb 18 22:41:46 2025 -0800

    Removing logal nuget. Fixing search finding issues.

[33mcommit 384b6c4097fb797554a794597beedbb3d7a2f23a[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Tue Feb 18 17:44:02 2025 -0800

    Adding call for requesting content data

[33mcommit 9504bd3fd24eb46d035a4bd009c8f5c64e53a492[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 18 19:42:21 2025 -0500

    Flag spots for search page updates

[33mcommit 550d9701e44b97c05d7aefe360e04917d724ac7a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 18 16:29:51 2025 -0500

    Add sign in check on client and AddUsersContributionRepositoriesToDatabaseAsync() after client updates

[33mcommit 39eb44d31b99a12b6909a5b0277f4b083a729fcf[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 18 10:37:31 2025 -0500

    Add dictionary to correct parsing errors

[33mcommit 956cf1faf2a53a39c2ccef44d68c4990598dc44f[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Fri Feb 14 17:04:04 2025 -0800

    Adding logging and data request

[33mcommit c6d4da0a5bbf6c59ffee5fcef9581461cc0e5fb4[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Fri Feb 14 16:34:34 2025 -0800

    Fixing stuff to make it build

[33mcommit a8be1491579cda801c8717592de13a60eb5792e8[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 14 15:17:14 2025 -0800

    Work in progress

[33mcommit 58c2d07f04df572a7de0eddba13b13e873519a37[m
Merge: a39ac5a 0c65965
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 14 15:04:57 2025 -0800

    Merge from dev - database

[33mcommit 0c65965b791dc0e095a1c52ce8170eeff050d298[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Fri Feb 14 14:43:25 2025 -0800

    User/felipeda/caching1 (#61)
    
    * Moved Data manager code
    
    * Added source branch to the database. Now need to fix query for multiple repos
    
    * Adding methods to get multiple repos data
    
    * Added async code to look into database
    
    * Fixing typo
    
    * Starting persistent data store
    
    * Persistently adding repositories
    
    * Using cache manager to deal with concurrency
    
    * Adding cache manager events and changing some stuff
    
    * Adding code for persistent data manager to deal with search objects
    
    * did stuff
    
    * Adding support for Search management
    
    * Changing the schemas to the new data type
    
    * Small fixes on schema
    
    * Refactoring some code and events
    
    * Search methods
    
    * typos
    
    ---------
    
    Co-authored-by: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>

[33mcommit a39ac5a8cec0675198df8bf2b004864152e3d769[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 17:07:27 2025 -0800

    [Draft] Add remove saved search command

[33mcommit 48968d4375d96055d6c2224bccd1ce28c8951241[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 17:01:40 2025 -0800

    Add name field to SaveSearchTemplate.json

[33mcommit 6c1acbc136935733e747bfb714674fedb1e557b1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 16:49:12 2025 -0800

    Add SearchHelper

[33mcommit 9aabc07b1353d5d18ed99b184437415e72816ae1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 15:23:12 2025 -0800

    Add type icon to SavedSearches page for each search

[33mcommit 895496b3658950dca9e6c5ed00afd7e5079c29ed[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 15:19:06 2025 -0800

    More cleanup: rename pullRequest icon string to pr, remove extra suppressions, and add type parsing in Search.cs

[33mcommit a928f2bd3d1d30a5ab94a419af5e3c4d9691bed7[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 14:10:40 2025 -0800

    Clean up SaveSearchForm and SaveSearchPage

[33mcommit 3f5755b415d6dd3e67df36b3ca30a63ed9791bb4[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 13:39:21 2025 -0800

    Rename query to search in most classes

[33mcommit 748611ab9c4ad0b1451e62bd36dcfe4c84c68fbe[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 13 11:05:26 2025 -0800

    Rename Query to Search throughout the extension (lowercase query will be in another commit)

[33mcommit 44d43e4b9d57677aed3c0fae17590176bc434619[m[33m ([m[1;31morigin/user/laurenciha/saved-queries-cleanup[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 12 18:26:01 2025 -0800

    Rename SaveQueryString to SaveQuery

[33mcommit 90ce538e5b18bab5b5ebc7609d834bbe7ae45cbc[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 12 18:17:20 2025 -0800

    Remove SaveQueryPage and SaveQueryForm and rename template to SaveQuerySurvey

[33mcommit c588e819c6f48d425457ac93183bf131332725c5[m[33m ([m[1;31morigin/user/laurenciha/saved-queries[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 12 17:10:24 2025 -0800

    Query errors propogate to the SaveQueryStringForm

[33mcommit 96ce9fa545ac921a8f54ce815c1d9d42c8060a97[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 12 16:57:33 2025 -0800

    Update form Query constructor to create query string

[33mcommit 28a919a037f719abb682c4dab9187c4319132426[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 12 16:08:16 2025 -0800

    Octokit supports query by string

[33mcommit e15333a6f3174a045e5d32bf1710eb563afbf32a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 11 10:36:59 2025 -0800

    Add search query by string form and page

[33mcommit bbeb051b90eaa3e802795bf0b4414dc6190e6e80[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 11 09:57:57 2025 -0800

    Make HandleSubmit void and run asynchronously

[33mcommit 6ede250e3b8308f7d9263447367031363592b1c1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 11 09:46:22 2025 -0800

    Remove unnecessary line in GitHubExtension.csproj

[33mcommit 6709bc28ecd17f4df8ea7a0ddd656b0a68ce2064[m
Merge: b254ae5 1efbe5e
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Feb 10 15:37:15 2025 -0800

    Merge pull request #52 from microsoft/dev
    
    GitHubExtension version 0.0.2

[33mcommit 1efbe5e65c6ed5fa5fbc14cf718e00b0f88200d9[m
Merge: 4de473d 7799eb3
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Feb 10 09:50:27 2025 -0800

    Merge pull request #18 from microsoft/user/laurenciha/saved-queries
    
    Add basic save queries functionality

[33mcommit 4de473de13d0a517861dd1429fe62aea06445eba[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Mon Feb 10 09:16:27 2025 -0800

    Adding last nuget package

[33mcommit 7799eb34890d9556020ca3e6942bc485e480bec6[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 17:59:23 2025 -0800

    Add PR filtering

[33mcommit 6649cb93fd8a35ec8165efd03db20af962543259[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 17:36:20 2025 -0800

    Saved queries are starting to work

[33mcommit 13311a5d8a3395a8c0dd7422d5350f57bc05a7c9[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 15:52:44 2025 -0800

    Add saved query to the SavedQueriesPage

[33mcommit 209e7af4d457f26fc37acc52ab6adcce15cebeed[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 15:16:54 2025 -0800

    Change dates to Input.Date

[33mcommit fde7a45ec3cbc81c6130e65674b9f592d962842b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 15:03:15 2025 -0800

    Basic parsing complete

[33mcommit 4a152ee53936e5b7c674fd66d2ad5af998d6aa3b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 14:09:08 2025 -0800

    First draft of SaveQuery adaptive card

[33mcommit 2d3d73a0ec5d9e4c2eada761b32578e810c28b0a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Feb 7 11:09:36 2025 -0800

    Add a template with data to SaveQueryForm

[33mcommit 953ff2bc03c81accfb9634b6b8f7c5bc2c4086a7[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 17:32:10 2025 -0800

    Add a debugging message to SaveQueryPage

[33mcommit 0fe95a67612dbe7021de629f804a5a120786942b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 15:39:20 2025 -0800

    Add QueryPage and move save query command to saved queries page

[33mcommit 5e34cad653d19df0194d5b4d62b9abc5124d2840[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 15:32:05 2025 -0800

    Add basic saved queries page

[33mcommit c283a2a9983944d987aac39fb30f740bee73110b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 15:19:33 2025 -0800

    Add basic save query page

[33mcommit b254ae5080bdaf2a41d53f093be34f8502380ec2[m
Merge: 63d8fb7 f39014c
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 6 14:07:28 2025 -0800

    Merge pull request #16 from microsoft/dev
    
    Updating the SDK to 0.0.5

[33mcommit f39014cd0707368831db95275213171dddbe89ab[m
Merge: eeee472 c457867
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Feb 6 14:07:02 2025 -0800

    Merge pull request #15 from microsoft/user/laurenciha/sdk0.0.5
    
    User/laurenciha/sdk0.0.5

[33mcommit c457867effc5341b08745f20b7b331a46bdf13a3[m[33m ([m[1;31morigin/user/laurenciha/sdk0.0.5[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 13:56:53 2025 -0800

    Update the nuget.config to Microsoft.CommandPalette.Extensions

[33mcommit 3cd21d34710058921529f4dd4062237cdddcb143[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 13:32:05 2025 -0800

    Delete old SDK reference, test classes, and swap AddRepoPage message for a toast

[33mcommit 61b62a04c238bdba1cd0a7224f07655a082205c6[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 10:25:18 2025 -0800

    Propagate more RepoHelpers (SDK migration 2/6)

[33mcommit 025b2fc8d9796c4f21c0414f6c1a899be4420c9b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Feb 6 09:43:58 2025 -0800

    Propogate Octokit errors to AddRepoPage

[33mcommit 697c866a9eefc118ece1987ccb825c375c0f6893[m[33m ([m[1;31morigin/user/laurenciha/rollback[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 5 16:17:46 2025 -0800

    More line endings and different SDK build

[33mcommit 8de539f9ec5f03f3dc14411447f18768f347e216[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 5 16:08:01 2025 -0800

    Revert Program.cs

[33mcommit a03eab97669413131018e2d8b7ca696ad87c96ab[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 5 14:59:12 2025 -0800

    Add debugging code

[33mcommit 53f1745672b0bbb4aefb28fcffce33d42f949508[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 5 14:17:30 2025 -0800

    Main is now called in new Command Palette

[33mcommit 6969bc42c0880f2a7a6955776e481c42babeeb58[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Feb 5 13:26:09 2025 -0800

    Update line endings in CommandsProvider

[33mcommit ba12a2741e06df634f9917c42f5645cec59aaf4f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 4 18:35:54 2025 -0800

    Remove Actions references

[33mcommit 790befcc9207442e6c0e2ea298b433aed5c35d45[m[33m ([m[1;31morigin/user/laurenciha/pop-ups[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 4 14:19:21 2025 -0800

    Remove page loading from synchronous signing out process

[33mcommit 72bdc4c8baad765cc537516c998d3baca6c8f004[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 4 13:59:02 2025 -0800

    Add sign out confirmation page

[33mcommit 44a2c4e8417241ce30ec9d7dd195b2397680bbb0[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Feb 4 13:42:02 2025 -0800

    Add sign out confirmation page

[33mcommit c5073aacc8abb6c99f079d6eb9b13b182d8d9296[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 16:14:02 2025 -0800

    Update test flow to match auth

[33mcommit 7d87e2661bcd34a7c765529ad33aa9e1ec543ab1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 15:30:40 2025 -0800

    Change auth events to static

[33mcommit 6178e9953e17409aec8870843a83a46a82936e2a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 13:12:53 2025 -0800

    Change test page/form to mimic auth page/flow. Success messages consistently appear.

[33mcommit f71a1ebb31003c9bfc49f58ed996eca902623b6a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 13:05:31 2025 -0800

    Add HideStatus logic to GitHubAuthPage

[33mcommit 3a1a8604d8f0910ebbe8ae965a72168fd9d9ee84[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 12:44:13 2025 -0800

    Hide add repo messages whenever the AddRepoPage opens

[33mcommit b7e2eb3b0866d707f6c25dcb45af04c7c69d27e9[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 12:43:43 2025 -0800

    Add note for code history

[33mcommit 2019fdfb5f246f75b198700af3424a7b98c928c3[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 12:29:18 2025 -0800

    Add test form page code for debugging

[33mcommit a46210ecab56c0fa938fe00a6951e8d86a46bf9f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Feb 3 12:29:03 2025 -0800

    Create one message per GitHubAuthPage

[33mcommit bc9ce893cade60f7e8f666b17249b43b99b20a77[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 16:59:10 2025 -0800

    Line ending update

[33mcommit ad55c86cd18926aec12821ae09f37aa435fb4935[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 16:48:50 2025 -0800

    Simplify message creation

[33mcommit 390a0ce4736c19fdd40d2e5aa081b233b307de96[m[33m ([m[1;31morigin/user/laurenciha/responsive[m[33m)[m
Merge: 9fc4ae4 e8fce54
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 31 16:19:57 2025 -0800

    Merge pull request #14 from microsoft/user/laurenciha/sdk-update
    
    Update Microsoft.CmdPal.Extensions.SDK.0.0.4.0

[33mcommit e8fce54dc82115ebfaf40bc2a81dff3671e91fff[m[33m ([m[1;31morigin/user/laurenciha/sdk-update[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 16:12:33 2025 -0800

    Update Microsoft.CmdPal.Extensions.SDK.0.0.4.0

[33mcommit 9fc4ae47cbbaa61d0e633bd42ef353c0160904a1[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 14:19:38 2025 -0800

    Loading animation plays while user is signing in

[33mcommit 6b77bf2b22e93abbf561d03fec099775d3b94df0[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 10:38:11 2025 -0800

    Clean up add repo page animation code

[33mcommit 07c83196db96d8e0737c587b61e72594fd142717[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 31 10:24:57 2025 -0800

    Responsive loading animation while repo is being added

[33mcommit a69fac074b82d784556481d92f1c3c2d323519db[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 30 17:57:26 2025 -0800

    Loading turns off and on!

[33mcommit eeee4727d1849a24b914819875ba16fd1a7a7861[m
Merge: 228eeb0 d107eca
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Thu Jan 30 14:08:23 2025 -0800

    Merge pull request #13 from microsoft/user/laurenciha/remove-public-fallback-from-issues-search
    
    User/laurenciha/remove public fallback from issues search

[33mcommit d107ecafc822aa1429f41a68c526dc960e6c5d3b[m[33m ([m[1;31morigin/user/laurenciha/remove-public-fallback-from-issues-search[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 30 14:07:25 2025 -0800

    Remove GitHubExtensionPackage

[33mcommit 6fa2a4b0f9ef0b2bdd88a0ab406d35d31c545b95[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 30 14:04:49 2025 -0800

    Clean up

[33mcommit 77c944e2faf67a8f1fd7bd28eea7c0f362953c59[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 30 13:53:41 2025 -0800

    Change sign in and out events to SignInStatusChangedEvent

[33mcommit edac3ab88aacc28f4612746021f5c5663f9f40f7[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 30 10:19:11 2025 -0800

    Return no issues found if repoCollection is empty

[33mcommit e28fad6e4b0ffd9442721d96c3b49ee753289f5f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 29 15:54:14 2025 -0800

    First attempt

[33mcommit 228eeb071510818b2d02d5643d1ba4cb582dd711[m
Merge: bace613 af31977
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Wed Jan 29 15:38:53 2025 -0800

    Merge branch 'user/felipeda/logging' into dev

[33mcommit bace6131dc2737d1f5ceea72226128eeaad70d8f[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Wed Jan 29 15:35:36 2025 -0800

    Removing x86

[33mcommit af31977081176c357251dcf0b05061690ab3adde[m[33m ([m[1;31morigin/user/felipeda/logging[m[33m)[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Wed Jan 29 15:29:55 2025 -0800

    Setting up logging stuff

[33mcommit 63d8fb70015c170ca68fa99793ad970a55486a11[m
Merge: 5ec4e0a 0b8384b
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Wed Jan 29 15:26:21 2025 -0800

    Merge pull request #12 from microsoft/dev
    
    Version 0.0.1

[33mcommit 0b8384b2f9a08f9232ebf0287b2dce905088f37a[m[33m ([m[1;31morigin/user/felipeda/removepackageapp[m[33m)[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Tue Jan 28 14:59:52 2025 -0800

    Adding property to generate package

[33mcommit b7d4a232cd758da56154fffb3adf72a531879b86[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Tue Jan 28 14:30:08 2025 -0800

    Removing packaging project

[33mcommit 9f3f17ca2774b9d88aac0e4af76d0da2e725bd3f[m[33m ([m[1;31morigin/user/felipeda/signing[m[33m)[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Tue Jan 28 11:22:14 2025 -0800

    Changing publisher and script

[33mcommit ab5af8ab880227a6d69c4bcb5a67c314ae33fda1[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Jan 27 16:41:39 2025 -0800

    Changing path of msix output

[33mcommit 33b91a05c98d63824dd8681adb8e454aae0473cd[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Jan 27 16:31:23 2025 -0800

    Fixing path

[33mcommit 42c8b6a21ff4e3599475a9318fb24f6dc23694c4[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Jan 27 16:19:11 2025 -0800

    Updating FolderPath

[33mcommit bb6baa43be682fbfcab62b080c2067ec79a645aa[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Mon Jan 27 16:03:45 2025 -0800

    Adding signing task

[33mcommit c5ea5800f406de6916f20be68e52a5d4b217c9da[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Mon Jan 27 15:26:17 2025 -0800

    Adding signing template

[33mcommit c074d106f30f3b63af725a90a92221175cecd67b[m
Merge: 2fc2634 4153441
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Jan 27 14:59:18 2025 -0800

    Merge pull request #11 from microsoft/user/laurenciha/sign-out
    
    Update RepositoryHelper sign in/out actions

[33mcommit 4153441611584c086f92e4e00df75aedf9b98692[m[33m ([m[1;31morigin/user/laurenciha/sign-out[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Jan 27 14:58:29 2025 -0800

    Update RepositoryHelper sign in/out actions

[33mcommit 2fc2634d49bcc2d4acfde593750a74fbcba398c7[m
Merge: e0dad1c 599dea1
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Mon Jan 27 14:11:57 2025 -0800

    Merge branch 'user/felipeda/build1' into dev

[33mcommit 599dea1ed3c06d3fea422e698c04729be78e4be8[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Mon Jan 27 14:06:59 2025 -0800

    adding deploy option

[33mcommit e0dad1c67cceb86967565efba4a6a8217f091046[m
Merge: dd546f1 47cf180
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Mon Jan 27 10:51:39 2025 -0800

    Merge pull request #10 from microsoft/user/laurenciha/add-repo-page
    
    Move add repo command to the top level

[33mcommit 47cf1806087a73b351e46e0fc1c8191d472f8570[m[33m ([m[1;31morigin/user/laurenciha/add-repo-page[m[33m)[m
Merge: 2ea0a55 dd546f1
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Jan 27 10:44:55 2025 -0800

    Merge branch 'dev' into user/laurenciha/add-repo-page

[33mcommit dd546f1d28e161c8384b79f5781c34363de229a7[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Mon Jan 27 10:29:10 2025 -0800

    Add ARM64 config

[33mcommit b0ae48d5c1e0445e5921648b9d48ec4037cf4eb0[m[33m ([m[1;31morigin/user/felipeda/refactoringpipeline1[m[33m)[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 22:43:13 2025 -0800

    Fixing msix name

[33mcommit 66b98de80422c8edb24713cca14bf1a498aae951[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 22:29:30 2025 -0800

    Removing first stage

[33mcommit c028599912a1b063ea24485788928c00220a58d8[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 21:42:22 2025 -0800

    Reorganizing stuff

[33mcommit 4adc7873ff01b8e8b72f7b29565a5de021b75c71[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 21:09:06 2025 -0800

    trying to separate into stages

[33mcommit 6e8ae68ae37a618b849eb7f1dac330373c577a93[m[33m ([m[1;31morigin/user/felipeda/packaging[m[33m, [m[1;31morigin/user/felipeda/build1[m[33m)[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 16:36:51 2025 -0800

    Adding root directory

[33mcommit ac0a1b030735768f65ccbb2927ccaa0204f5a134[m
Merge: bf50cfb e93f322
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 16:32:03 2025 -0800

    Merge branch 'user/felipeda/packaging' of github.com:microsoft/CmdPalGitHubExtension into user/felipeda/packaging

[33mcommit bf50cfb912e25834b8bb364462dee8e2e03406a5[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 16:31:49 2025 -0800

    Small fix on script

[33mcommit e93f32266297f5123ef75ee5e5fcfc300f7f3d6d[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 16:28:48 2025 -0800

    Passing params to oauth script

[33mcommit c2092867620b09044e40f7a5023f076d32603ca9[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 16:27:11 2025 -0800

    Adding params

[33mcommit 5f46ed9bc05afa6d3d90207a9285516828932868[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 16:08:14 2025 -0800

    Update azure-pipelines.yml for Azure Pipelines

[33mcommit 745f9ddb306ab84ae54c3ea4a38cb76a69cd7c94[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 16:06:44 2025 -0800

    Adding OAuth script

[33mcommit abe83b71ab771a463b48e51ca2dd86df648a166d[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 15:44:58 2025 -0800

    Adding packaging project

[33mcommit c3dc35ebbd09c850c660e23cc423ac9889959c91[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Sat Jan 25 15:02:40 2025 -0800

    Adding properties files

[33mcommit 183fa163875212fb43f9d08b3d67c5f52d27b14a[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 14:45:01 2025 -0800

    Removing previous build task

[33mcommit 0c37ce6bba5bc67001e762d154261ece334a0ae3[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Sat Jan 25 14:31:46 2025 -0800

    Editing msix task

[33mcommit c4e37babe62005c813b592037307711286932d6a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 20:04:22 2025 -0800

    Add properties directory

[33mcommit 2ea0a559ee49882a0bc44166527f5808d0c4dd46[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 19:16:38 2025 -0800

    Move AddRepoCommand to the top-level commands

[33mcommit fd60d0f404f9f46e25683913b17933205a4bf6bd[m
Merge: 2cf4e52 d403f26
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 18:57:42 2025 -0800

    Merge pull request #9 from microsoft/user/laurenciha/add-repo-page
    
    User/laurenciha/add repo page

[33mcommit d403f26dde988955138d297ca2ff946b4b7beb9b[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 18:56:26 2025 -0800

    Allow public repos to be added

[33mcommit 066bd5930babc4920297ae66540a84075214f581[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 18:49:38 2025 -0800

    Add repo by URL

[33mcommit 18f65475a77cedd4772322e4cdabe82647554207[m
Merge: c9e41f0 2cf4e52
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 18:33:36 2025 -0800

    Merge branch 'dev' into user/laurenciha/add-repo-page

[33mcommit 2cf4e52330e8a6847141896b45eb45dd4cf85b7f[m
Merge: a61c9eb ca2f82d
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 17:30:10 2025 -0800

    Merge pull request #7 from microsoft/user/laurenciha/issues
    
    Remove organization command from search issues page

[33mcommit ca2f82de148e15eaeca6bf3c7ef82678d4f915c3[m[33m ([m[1;31morigin/user/laurenciha/issues[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 17:29:17 2025 -0800

    Remove organization command from search issues page

[33mcommit a61c9eb3cb329211df849368943193b7d579e6a3[m
Merge: 5c4b4dc 2600d86
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 17:21:25 2025 -0800

    Merge pull request #6 from microsoft/user/laurenciha/issues
    
    First iteration of search issues page

[33mcommit 5c4b4dc050c26c77029122f7891f756d0bca57b5[m
Merge: 3d08a1b a9c9195
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 17:20:40 2025 -0800

    Merge pull request #5 from microsoft/user/laurenciha/prs
    
    Remove details from search prs page

[33mcommit a9c9195232a04f111e64b9a8f5b542dd9877be99[m[33m ([m[1;31morigin/user/laurenciha/prs[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 17:19:54 2025 -0800

    Remove details from search prs page

[33mcommit 2600d86b5a721316a6ab601960b40fd0e5228f7d[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 17:15:44 2025 -0800

    Remove issue details

[33mcommit 3d08a1b7f97dea753af367fd69cdb1b9e9b4c340[m
Merge: a9b8a6d ebbfeb3
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 17:04:55 2025 -0800

    Merge pull request #4 from microsoft/main
    
    First iteration of search pull requests feature

[33mcommit ebbfeb31a46522701e73a36a77c8a1dc9cdb646f[m
Merge: 5ec4e0a 76a6f83
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 17:02:21 2025 -0800

    Merge pull request #3 from microsoft/user/laurenciha/prs
    
    First iteration of search pull requests feature

[33mcommit 76a6f83fcebc686c4385af91a2b7cd3ebc6fc2c0[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 16:52:21 2025 -0800

    Add ability to copy source branch name (for easy checkout)

[33mcommit c5bfb93cc0c7d4db6043a6ae1320d4e521d04b3c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 16:49:10 2025 -0800

    Add support for Octokit PullRequest objects

[33mcommit 4b16ba130437c3ba2509c5e40db97cdfc40c9438[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 16:18:20 2025 -0800

    Simplify link command so any data object can use it

[33mcommit e23b3ffc5514348efae156bf43812f36fb3baa65[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 16:12:07 2025 -0800

    Remove organization item from search pull requests page

[33mcommit 81905f18143bfff087d5f67364653111aea2f847[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 16:06:01 2025 -0800

    Update search options and add default values to DataObjects/PullRequests

[33mcommit 02b2275cb3b7ae0823933c92575265b2fd7fef73[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 15:33:57 2025 -0800

    Remove GetAllIssuesAsync() in SearchIssuesPage

[33mcommit c9e41f0e975ac203c674e2e8e157325f56f9d21f[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 15:29:55 2025 -0800

    Add Repository list to GitHubRepositoryHelper

[33mcommit a9daf851ec1f263ef976328fad26aaa96c9cf0e4[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 12:14:45 2025 -0800

    Add pagination outline and Owner/Collaborator filter on repos

[33mcommit 2fd07f15f62427b9c5f5f8b079b843b430099a96[m
Merge: b472c45 a9b8a6d
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 24 10:48:22 2025 -0800

    Merge branch 'dev' into user/laurenciha/add-repo-page

[33mcommit a9b8a6dab3d6b69614a859ef7d9d14a66442a87b[m
Merge: 88caa2a 5ab75d2
Author: Lauren Ciha <64796985+lauren-ciha@users.noreply.github.com>
Date:   Fri Jan 24 10:44:55 2025 -0800

    Merge pull request #2 from microsoft/user/laurenciha/sdk
    
    Adding in recent changes and extension SDK update to 0.2.0

[33mcommit 58525425690aa596509d4613629ae9cc055211a9[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 23 21:03:55 2025 -0800

    Add pagination to the repository request

[33mcommit 6791f311ffa7728e2d3a28f3fb4804cfac59b9b1[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 23:12:39 2025 -0800

    Building only x64

[33mcommit da485ca310c94edfd0bde2e6777887e371002d40[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 23:04:43 2025 -0800

    Trying msix task

[33mcommit b472c45a024b1be7bf601c1fd7d521323eeda1e3[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 20:54:56 2025 -0800

    Add command to add repo and fix getting repo name and owner

[33mcommit 57084dc9c88a4ee9d350cf4440dbe417630f7ded[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 20:20:40 2025 -0800

    Adding second msbuild command

[33mcommit 2fef91c08a87d961a48e50f9de4515ac8235b7dc[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 20:19:02 2025 -0800

    Add Search Pull Requests Command

[33mcommit 30dd6cd3fdfec3743e98660dde0c99bafb26f32c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 18:38:59 2025 -0800

    Add starter pull request classes

[33mcommit dfa45577eeadf9d756c67694ff9014459068305b[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 19:55:38 2025 -0800

    Adding copy files

[33mcommit 4813d76b3981424ab920d7a6fe6eec703532074b[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 19:41:45 2025 -0800

    Adding publish artifacts

[33mcommit f63e6db17c6584dbcde1de1c101bed69d101a376[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 18:35:24 2025 -0800

    Using msbuild argument for restore

[33mcommit d3261397911f500e0f8f337b6e71301e269bc9a3[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 18:04:26 2025 -0800

    Add restore to msbuild task

[33mcommit 9f3163aaf5f092f497420f6ca524ecc5932d49d5[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 17:44:53 2025 -0800

    Change nuget.config path 2

[33mcommit 6bc6bd3b1635f32b49f9cdc98b10b8105ac71485[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 17:38:38 2025 -0800

    Change nuget config path

[33mcommit e3f849a7e453ea602bbaa227aa75e61af0b33ca4[m
Author: Felipe G <guima.felipec@gmail.com>
Date:   Wed Jan 22 17:13:49 2025 -0800

    Update azure-pipelines.yml for Azure Pipelines

[33mcommit a4cc1f70cbd88dae798122dd4a76226a9929141c[m
Merge: 5ab75d2 5ec4e0a
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Wed Jan 22 16:16:16 2025 -0800

    Merge branch 'main' into user/felipeda/build1

[33mcommit 6360b8ea1e79c7f44f00fb184d1e0786c07d850c[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 16:08:07 2025 -0800

    Add temporary way to save repo information manually entered

[33mcommit c7b27cb6d20515568032f588a461e6b9d7106ba6[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 16:00:16 2025 -0800

    Add AddRepoForm and Page

[33mcommit 33e40bb2e4f23a2e290310ad185abfc54dd8e96a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 15:48:33 2025 -0800

    Attempt batching and tweak image

[33mcommit 135c4b388f3b298f729fad5609c406e777802559[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 15:17:16 2025 -0800

    Start searching by organization (few, public repos for now)

[33mcommit fb9b6e66d5656052abd148048d9b611936a77363[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 15:15:53 2025 -0800

    Expand permissions to debug Octokit errors

[33mcommit 5ab75d20b221896c95255c0ceb03c370930a738a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 14:42:32 2025 -0800

    New SDK!

[33mcommit 9ca7541b265fd118fc96e540ddf7b6bd738f5c53[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 13:12:11 2025 -0800

    Add private repositories to issue query

[33mcommit 071170d550395ebe8ef1b6275f3be5ed72793b77[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Wed Jan 22 12:57:22 2025 -0800

    Change from getting assigned issues to a query

[33mcommit cf4ab221142eaeeb171852c3885fd9325f261cfa[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Jan 21 18:00:43 2025 -0800

    Limit number of items returned and change default filtering (throws errors)

[33mcommit ba3c2dadadc8c056d8e7809f92f1e9615680891a[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Jan 21 17:33:06 2025 -0800

    Add error throws so messages propogate to user

[33mcommit be6ea7d4315665d22047d5061dd78942ff8c13ca[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Jan 21 15:54:02 2025 -0800

    Update Microsoft.CmdPal.Extensions.SDK to 0.0.2

[33mcommit a82141312451daa51bcbb70d1fe9ead60f315edd[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 17 15:07:02 2025 -0800

    Add in SearchIssueRequest options

[33mcommit 5ec4e0a0a63406cde36c7b92528599c63c663f73[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Tue Jan 21 10:42:15 2025 -0800

    Add yaml template

[33mcommit 1b819dd4230afbc39811426d9e30a920c1b71309[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 17 11:47:25 2025 -0800

    Make open link the default command and remove tags from search issue items

[33mcommit 206031a20b5fc9fe126183a36714d34d4b8b4b42[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 17 11:37:50 2025 -0800

    Add repo name to subtitle

[33mcommit a589bf583dea6770d0f0dc2bfe373e2a8ce06c50[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 17 11:34:02 2025 -0800

    Change api url to html url

[33mcommit 0e3236f9133b732f67580198af52c307ebd95efd[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Fri Jan 17 11:19:51 2025 -0800

    Refactor, update SearchIssues subtitle to owner/repo/issue#, and create GitHubAPI validation method

[33mcommit 88caa2a605661506334d8dcc4c29f3c4fcea7929[m[33m ([m[1;31morigin/user/laurenciha/login[m[33m)[m
Author: Lauren Ciha <laurenciha@microsoft.com>
Date:   Thu Jan 16 17:59:13 2025 -0800

    Initial commit for login code

[33mcommit 91b2e3b3bef694454d01b7b3b942255fa5d42d84[m[33m ([m[1;31morigin/user/felipeda/movingstuff[m[33m)[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Thu Jan 16 16:54:24 2025 -0800

    putting back nuget packages on git ignore

[33mcommit 0f4817e7a5e1f7da0247b9a77af87ab71ef2a6e6[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Thu Jan 16 16:52:51 2025 -0800

    Fixing package name

[33mcommit 2092e062ec3e7f391b3be8364c83cd8500e650a9[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Thu Jan 16 16:40:20 2025 -0800

    Putting the nuget package on source code

[33mcommit 02fdf60744d1d276a48800bb2115cb644d2d0b52[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Thu Jan 16 16:19:38 2025 -0800

    Changing git ignore and nuget config to have SDK nuget packages

[33mcommit ea99379e28774830f3bd4e0c68c27ec4564d8108[m
Author: Felipe da Conceicao Guimaraes <felipeda@microsoft.com>
Date:   Thu Jan 16 10:22:53 2025 -0800

    Moving project to this repo

[33mcommit 5533c4ca509c21b3d003bbd554c503c072e735ce[m
Author: Microsoft Open Source <microsoftopensource@users.noreply.github.com>
Date:   Wed Jan 8 13:16:31 2025 -0800

    SECURITY.md committed

[33mcommit 069eb5015e6a415e56c01a1600fc85192f41ef86[m
Author: Microsoft Open Source <microsoftopensource@users.noreply.github.com>
Date:   Wed Jan 8 13:16:30 2025 -0800

    README.md committed

[33mcommit ee934514b875321ef4ad6af7cc40dc5535309dae[m
Author: Microsoft Open Source <microsoftopensource@users.noreply.github.com>
Date:   Wed Jan 8 13:16:29 2025 -0800

    SUPPORT.md committed

[33mcommit b5af62bab23932d053fb77f835790dc6288fa3ee[m
Author: Microsoft Open Source <microsoftopensource@users.noreply.github.com>
Date:   Wed Jan 8 13:16:28 2025 -0800

    LICENSE committed

[33mcommit 6c1b422001feca6c2269ea67a7f26b8db2a88f47[m
Author: Microsoft Open Source <microsoftopensource@users.noreply.github.com>
Date:   Wed Jan 8 13:16:27 2025 -0800

    CODE_OF_CONDUCT.md committed

[33mcommit 2f7f768531ddd87f3b0fc2e2f4b592141136b8ff[m
Author: microsoft-github-operations[bot] <55726097+microsoft-github-operations[bot]@users.noreply.github.com>
Date:   Wed Jan 8 21:16:25 2025 +0000

    Initial commit
